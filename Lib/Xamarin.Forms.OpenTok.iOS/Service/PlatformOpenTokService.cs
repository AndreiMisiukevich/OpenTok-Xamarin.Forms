using System;
using System.ComponentModel;
using Xamarin.Forms.OpenTok.Service;
using AVFoundation;
using OpenTok;
using Foundation;
using System.Threading.Tasks;

namespace Xamarin.Forms.OpenTok.iOS.Service
{
    [Preserve(AllMembers = true)]
    public sealed class PlatformOpenTokService : BaseOpenTokService
    {
        public event Action PublisherUpdated;
        public event Action SubscriberUpdated;

        private readonly object _locker = new object();

        private PlatformOpenTokService()
        {
            PropertyChanged += OnPropertyChanged;
        }

        public static PlatformOpenTokService Instance => CrossOpenTok.Current as PlatformOpenTokService;

        public OTSession Session { get; private set; }
        public OTPublisher PublisherKit { get; private set; }
        public OTSubscriber SubscriberKit { get; private set; }

        public void ClearPublisherUpdated() => PublisherUpdated = null;

        public void ClearSubscribeUpdated() => SubscriberUpdated = null;

        public static void Init()
        {
            OpenTokPublisherViewRenderer.Preserve();
            OpenTokSubscriberViewRenderer.Preserve();
            CrossOpenTok.Init(() => new PlatformOpenTokService());
        }

        public override bool TryStartSession()
        {
            if(!CheckPermissions() ||
                string.IsNullOrWhiteSpace(ApiKey) ||
                string.IsNullOrWhiteSpace(SessionId) ||
                string.IsNullOrWhiteSpace(UserToken))
            {
                return false;
            }

            IsSessionStarted = true;
            EndSession();

            Session = new OTSession(ApiKey, SessionId, null);
            Session.ConnectionDestroyed += OnConnectionDestroyed;
            Session.DidConnect += OnDidConnect;
            Session.StreamCreated += OnStreamCreated;
            Session.StreamDestroyed += OnStreamDestroyed;
            Session.DidFailWithError += OnError;
            Session.ReceivedSignalType += OnSignalReceived;
            Session.Init();

            Session.ConnectWithToken(UserToken, out OTError error);
            return true;
        }

        public override void EndSession()
        {
            try
            {
                if (Session == null)
                {
                    return;
                }

                lock (_locker)
                {
                    if (SubscriberKit != null)
                    {
                        SubscriberKit.SubscribeToAudio = false;
                        SubscriberKit.SubscribeToVideo = false;
                        SubscriberKit.DidConnectToStream -= OnSubscriberDidConnectToStream;
                        SubscriberKit.VideoDataReceived -= OnSubscriberVideoDataReceived;
                        SubscriberKit.VideoEnabled -= OnSubscriberVideoEnabled;
                        SubscriberKit.VideoDisabled -= OnSubscriberVideoDisabled;
                        SubscriberKit.Dispose();
                        SubscriberKit = null;
                    }

                    if (PublisherKit != null)
                    {
                        PublisherKit.PublishAudio = false;
                        PublisherKit.PublishVideo = false;
                        PublisherKit.StreamCreated -= OnPublisherStreamCreated;
                        PublisherKit.Dispose();
                        PublisherKit = null;
                    }

                    if (Session != null)
                    {
                        Session.ConnectionDestroyed -= OnConnectionDestroyed;
                        Session.DidConnect -= OnDidConnect;
                        Session.StreamCreated -= OnStreamCreated;
                        Session.StreamDestroyed -= OnStreamDestroyed;
                        Session.DidFailWithError -= OnError;
                        Session.ReceivedSignalType -= OnSignalReceived;
                        Session.Disconnect();
                        Session.Dispose();
                        Session = null;
                    }
                }
            }
            finally
            {
                IsSessionStarted = false;
                IsPublishingStarted = false;
            }
        }

        public override bool CheckPermissions() => true;

        public override Task<bool> SendMessageAsync(string message)
        {
            Session.SignalWithType(string.Empty, message, Session.Connection, out OTError error);
            return Task.FromResult(error == null);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PublisherKit != null)
            {
                switch (e.PropertyName)
                {
                    case nameof(IsVideoPublishingEnabled):
                        PublisherKit.PublishVideo = IsVideoPublishingEnabled;
                        return;
                    case nameof(IsAudioPublishingEnabled):
                        PublisherKit.PublishAudio = IsAudioPublishingEnabled;
                        return;
                }
            }
            if(SubscriberKit != null)
            {
                switch(e.PropertyName)
                {
                    case nameof(IsVideoSubscriptionEnabled):
                        SubscriberKit.SubscribeToVideo = IsVideoSubscriptionEnabled;
                        return;
                    case nameof(IsAudioSubscriptionEnabled):
                        SubscriberKit.SubscribeToAudio = IsAudioSubscriptionEnabled;
                        return;
                }
            }
        }

        public override void CycleCamera()
        {
            if (PublisherKit == null)
            {
                return;
            }

            PublisherKit.CameraPosition = PublisherKit.CameraPosition == AVCaptureDevicePosition.Front
                ? AVCaptureDevicePosition.Back
                : AVCaptureDevicePosition.Front;
        }

        private void OnConnectionDestroyed(object sender, OTSessionDelegateConnectionEventArgs e) => EndSession();

        private void OnDidConnect(object sender, EventArgs e)
        {
            lock (_locker)
            {
                if (Session == null || PublisherKit != null)
                {
                    return;
                }

                PublisherKit = new OTPublisher(null, new OTPublisherSettings
                {
                    Name = "XamarinOpenTok",
                    CameraFrameRate = OTCameraCaptureFrameRate.OTCameraCaptureFrameRate15FPS,
                    CameraResolution = OTCameraCaptureResolution.High,
                })
                {
                    PublishVideo = IsVideoPublishingEnabled,
                    PublishAudio = IsAudioPublishingEnabled
                };
                PublisherKit.StreamCreated += OnPublisherStreamCreated;

                Session.Publish(PublisherKit, out OTError error);
                PublisherUpdated?.Invoke();
            }
        }

        private void OnStreamCreated(object sender, OTSessionDelegateStreamEventArgs e)
        {
            lock (_locker)
            {
                if (Session == null || SubscriberKit != null)
                {
                    return;
                }

                SubscriberKit = new OTSubscriber(e.Stream, null)
                {
                    SubscribeToVideo = IsVideoSubscriptionEnabled
                };
                SubscriberKit.SubscribeToVideo = IsAudioSubscriptionEnabled;
                SubscriberKit.DidConnectToStream += OnSubscriberDidConnectToStream;
                SubscriberKit.VideoDataReceived += OnSubscriberVideoDataReceived;
                SubscriberKit.VideoEnabled += OnSubscriberVideoEnabled;
                SubscriberKit.VideoDisabled += OnSubscriberVideoDisabled;

                Session.Subscribe(SubscriberKit, out OTError error);
            }
        }

        private void OnStreamDestroyed(object sender, OTSessionDelegateStreamEventArgs e) => SubscriberUpdated?.Invoke();

        private void OnError(object sender, OTSessionDelegateErrorEventArgs e)
        {
            RaiseError(e.Error?.Code.ToString());
            EndSession();
        }

        private void OnSubscriberVideoDisabled(object sender, OTSubscriberKitDelegateVideoEventReasonEventArgs e)
        => IsSubscriberVideoEnabled = false;

        private void OnSubscriberVideoDataReceived(object sender, EventArgs e)
        => SubscriberUpdated?.Invoke();

        private void OnSubscriberVideoEnabled(object sender, OTSubscriberKitDelegateVideoEventReasonEventArgs e)
        {
            lock (_locker)
            {
                IsSubscriberVideoEnabled = SubscriberKit?.Stream?.HasVideo ?? false;
            }
        }

        private void OnSubscriberDidConnectToStream(object sender, EventArgs e)
        {
            lock (_locker)
            {
                if (SubscriberKit != null)
                {
                    SubscriberUpdated?.Invoke();
                    IsSubscriberVideoEnabled = SubscriberKit?.Stream?.HasVideo ?? false;
                    PublisherUpdated?.Invoke();
                }
            }
        }

        private void OnPublisherStreamCreated(object sender, OTPublisherDelegateStreamEventArgs e)
        => IsPublishingStarted = true;

        private void OnSignalReceived(object sender, OTSessionDelegateSignalEventArgs e)
        => RaiseMessageReceived(e.StringData);
    }
}
