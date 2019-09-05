using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Xamarin.Forms.OpenTok.Service;
using AVFoundation;
using OpenTok;
using Foundation;
using System.Threading.Tasks;

namespace Xamarin.Forms.OpenTok.iOS.Service
{
    public class OpenTokUserUpdatedEventArgs : EventArgs
    {
        public string StreamId { get; }

        public OpenTokUserUpdatedEventArgs(string streamId)
        {
            StreamId = streamId;
        }
    }

    [Preserve(AllMembers = true)]
    public sealed class PlatformOpenTokService : BaseOpenTokService
    {
        public event EventHandler<OpenTokUserUpdatedEventArgs> PublisherUpdated;
        public event EventHandler<OpenTokUserUpdatedEventArgs> SubscriberUpdated;

        private readonly object _locker = new object();

        private PlatformOpenTokService()
        {
            PropertyChanged += OnPropertyChanged;
            Subscribers = new Dictionary<string, OTSubscriber>();
        }

        public static PlatformOpenTokService Instance => CrossOpenTok.Current as PlatformOpenTokService;

        private OTSession Session { get; set; }
        public OTPublisher PublisherKit { get; private set; }
        public Dictionary<string, OTSubscriber> Subscribers { get; private set; }

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

            Session.ConnectWithToken(UserToken, out OTError _);
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
                    if (Subscribers?.Count > 0)
                    {
                        foreach (OTSubscriber subscriberKit in Subscribers.Select(subscriber => subscriber.Value))
                        {
                            subscriberKit.SubscribeToAudio = false;
                            subscriberKit.SubscribeToVideo = false;
                            subscriberKit.DidConnectToStream -= OnSubscriberConnected;
                            subscriberKit.DidDisconnectFromStream -= OnSubscriberDisconnected;
                            subscriberKit.VideoDataReceived -= OnSubscriberVideoDataReceived;
                            subscriberKit.VideoEnabled -= OnSubscriberVideoEnabled;
                            subscriberKit.VideoDisabled -= OnSubscriberVideoDisabled;
                            subscriberKit.Dispose();
                        }

                        Subscribers = new Dictionary<string, OTSubscriber>();
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
            Session.SignalWithType(string.Empty, message, null, out OTError error);

            bool messageSent = error == null;

            error?.Dispose();
            return Task.FromResult(messageSent);
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
            if (Subscribers?.Count > 0)
            {
                switch (e.PropertyName)
                {
                    case nameof(IsVideoSubscriptionEnabled):
                        //TODO: This is probably wrong.
                        foreach (OTSubscriber subscriberKit in Subscribers.Select(subscriber => subscriber.Value))
                        {
                            subscriberKit.SubscribeToVideo = IsVideoSubscriptionEnabled;
                        }

                        return;
                    case nameof(IsAudioSubscriptionEnabled):
                        //TODO: This is probably wrong.
                        foreach (OTSubscriber subscriberKit in Subscribers.Select(subscriber => subscriber.Value))
                        {
                            subscriberKit.SubscribeToAudio = IsAudioSubscriptionEnabled;
                        }

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

                Session.Publish(PublisherKit);
                PublisherUpdated?.Invoke(sender, new OpenTokUserUpdatedEventArgs(PublisherKit.Session.SessionId));
            }
        }

        private void OnStreamCreated(object sender, OTSessionDelegateStreamEventArgs e)
        {
            lock (_locker)
            {
                if (Session == null || Subscribers != null &&
                    Subscribers.ContainsKey(e.Stream.StreamId))
                {
                    return;
                }

                var subscriberKit = new OTSubscriber(e.Stream, null)
                {
                    SubscribeToVideo = IsVideoSubscriptionEnabled,
                    SubscribeToAudio = IsAudioSubscriptionEnabled
                };
                subscriberKit.DidConnectToStream += OnSubscriberConnected;
                subscriberKit.DidDisconnectFromStream += OnSubscriberDisconnected;
                subscriberKit.VideoDataReceived += OnSubscriberVideoDataReceived;
                subscriberKit.VideoEnabled += OnSubscriberVideoEnabled;
                subscriberKit.VideoDisabled += OnSubscriberVideoDisabled;
                Session.Subscribe(subscriberKit);

                Subscribers?.Add(e.Stream.StreamId, subscriberKit);
                OnSubscriberAdded(e.Stream.StreamId);
            }
        }

        private void OnStreamDestroyed(object sender, OTSessionDelegateStreamEventArgs e) => SubscriberUpdated?.Invoke(sender, new OpenTokUserUpdatedEventArgs(e.Stream.StreamId));

        private void OnError(object sender, OTSessionDelegateErrorEventArgs e)
        {
            RaiseError(e.Error?.Code.ToString(CultureInfo.CurrentUICulture));
            EndSession();
        }

        private void OnSubscriberVideoDisabled(object sender, OTSubscriberKitDelegateVideoEventReasonEventArgs e)
        => IsSubscriberVideoEnabled = false;

        private void OnSubscriberVideoDataReceived(object sender, EventArgs e)
        => SubscriberUpdated?.Invoke(sender, new OpenTokUserUpdatedEventArgs(((OTSubscriber)sender).Stream.StreamId));

        private void OnSubscriberVideoEnabled(object sender, OTSubscriberKitDelegateVideoEventReasonEventArgs e)
        {
            lock (_locker)
            {
                var subscriberKit = (OTSubscriber) sender;
                IsSubscriberVideoEnabled = subscriberKit?.Stream?.HasVideo ?? false;
            }
        }

        private void OnSubscriberConnected(object sender, EventArgs e) => OnSubscriberConnectionChanged(true, (OTSubscriber)sender);

        private void OnSubscriberDisconnected(object sender, EventArgs e) => OnSubscriberConnectionChanged(false, (OTSubscriber)sender);

        private void OnSubscriberConnectionChanged(bool isConnected, OTSubscriber subscriberKit)
        {
            lock (_locker)
            {
                if (subscriberKit?.Stream != null)
                {
                    if (Subscribers.ContainsKey(subscriberKit.Stream.StreamId))
                    {
                        SubscriberUpdated?.Invoke(this, new OpenTokUserUpdatedEventArgs(subscriberKit.Stream.StreamId));
                        IsSubscriberConnected = isConnected;
                        IsSubscriberVideoEnabled = subscriberKit.Stream?.HasVideo ?? false;
                        PublisherUpdated?.Invoke(this, new OpenTokUserUpdatedEventArgs(PublisherKit.Session.SessionId));
                    }
                }
            }
        }

        private void OnPublisherStreamCreated(object sender, OTPublisherDelegateStreamEventArgs e)
        => IsPublishingStarted = true;

        private void OnSignalReceived(object sender, OTSessionDelegateSignalEventArgs e)
        => RaiseMessageReceived(e.StringData);

        private void OnSubscriberAdded(string streamId)
            => RaiseSubscriberAdded(streamId);
    }
}
