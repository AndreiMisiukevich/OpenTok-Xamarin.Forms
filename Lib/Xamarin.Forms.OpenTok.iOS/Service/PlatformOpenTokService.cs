using System;
using System.ComponentModel;
using Xamarin.Forms.OpenTok.Service;
using AVFoundation;
using OpenTok;

namespace Xamarin.Forms.OpenTok.iOS.Service
{
    public sealed class PlatformOpenTokService : OpenTokService
    {
        public event EventHandler PublisherUpdated;
        public event EventHandler SubscriberUpdated;

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
            CrossOpenTok.Initialize(() => new PlatformOpenTokService());
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
            Session.Init();

            OTError error;
            Session.ConnectWithToken(UserToken, out error);
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

                OTError error;
                PublisherKit = new OTPublisher(null, new OTPublisherSettings
                {
                    Name = "XamarinOpenTok",
                    CameraFrameRate = OTCameraCaptureFrameRate.OTCameraCaptureFrameRate15FPS,
                    CameraResolution = OTCameraCaptureResolution.High,
                });

                PublisherKit.PublishVideo = IsVideoPublishingEnabled;
                PublisherKit.PublishAudio = IsAudioPublishingEnabled;
                PublisherKit.StreamCreated += OnPublisherStreamCreated;

                Session.Publish(PublisherKit, out error);
                PublisherUpdated?.Invoke(this, EventArgs.Empty);
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

                SubscriberKit = new OTSubscriber(e.Stream, null);
                SubscriberKit.SubscribeToVideo = IsVideoSubscriptionEnabled;
                SubscriberKit.SubscribeToVideo = IsAudioSubscriptionEnabled;
                SubscriberKit.DidConnectToStream += OnSubscriberDidConnectToStream;
                SubscriberKit.VideoDataReceived += OnSubscriberVideoDataReceived;
                SubscriberKit.VideoEnabled += OnSubscriberVideoEnabled;
                SubscriberKit.VideoDisabled += OnSubscriberVideoDisabled;

                OTError error;
                Session.Subscribe(SubscriberKit, out error);
            }
        }

        private void OnStreamDestroyed(object sender, OTSessionDelegateStreamEventArgs e) => SubscriberUpdated?.Invoke(this, EventArgs.Empty);

        private void OnError(object sender, OTSessionDelegateErrorEventArgs e)
        {
            RaiseError(e.Error.Description);
            EndSession();
        }

        private void OnSubscriberVideoDisabled(object sender, OTSubscriberKitDelegateVideoEventReasonEventArgs e)
        => IsSubscriberVideoEnabled = false;

        private void OnSubscriberVideoDataReceived(object sender, EventArgs e)
        => SubscriberUpdated?.Invoke(this, EventArgs.Empty);

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
                    SubscriberUpdated?.Invoke(this, EventArgs.Empty);
                    IsSubscriberVideoEnabled = SubscriberKit?.Stream?.HasVideo ?? false;
                    PublisherUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void OnPublisherStreamCreated(object sender, OTPublisherDelegateStreamEventArgs e)
        => IsPublishingStarted = true;
    }
}
