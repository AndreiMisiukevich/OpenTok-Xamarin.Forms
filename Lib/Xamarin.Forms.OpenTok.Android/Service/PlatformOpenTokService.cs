using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.Content.PM;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Com.Opentok.Android;
using Plugin.CurrentActivity;
using Xamarin.Forms.OpenTok.Service;

namespace Xamarin.Forms.OpenTok.Android.Service
{
    [Preserve(AllMembers = true)]
    public sealed class PlatformOpenTokService : BaseOpenTokService
    {
        public event Action PublisherUpdated;
        public event Action SubscriberUpdated;

        private const int RequestId = 0;

        private readonly string[] _requestPermissions = {
            Manifest.Permission.Camera,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.RecordAudio,
            Manifest.Permission.ModifyAudioSettings,
            Manifest.Permission.Internet,
            Manifest.Permission.AccessNetworkState
        };

        private readonly object _locker = new object();

        private PlatformOpenTokService()
        {
            PropertyChanged += OnPropertyChanged;
        }

        public static PlatformOpenTokService Instance => CrossOpenTok.Current as PlatformOpenTokService;

        public Session Session { get; private set; }
        public PublisherKit PublisherKit { get; private set; }
        public SubscriberKit SubscriberKit { get; private set; }

        public void ClearPublisherUpdated() => PublisherUpdated = null;

        public void ClearSubscribeUpdated() => SubscriberUpdated = null;

        public static void Init()
        {
            OpenTokPublisherViewRenderer.Preserve();
            OpenTokSubscriberViewRenderer.Preserve();
            CrossOpenTok.Init(() => new PlatformOpenTokService());
        }

        public override bool CheckPermissions()
        {
            var activity = CrossCurrentActivity.Current.Activity;
            var shouldGrantPermissions = _requestPermissions.Any(permission => ContextCompat.CheckSelfPermission(activity, permission) != (int)Permission.Granted);
            if (shouldGrantPermissions)
            {
                ActivityCompat.RequestPermissions(activity, _requestPermissions, RequestId);
            }
            return !shouldGrantPermissions;
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

            Session = new Session.Builder(CrossCurrentActivity.Current.AppContext, ApiKey, SessionId).Build();
            Session.ConnectionDestroyed += OnConnectionDestroyed;
            Session.Connected += OnDidConnect;
            Session.StreamReceived += OnStreamCreated;
            Session.StreamDropped += OnStreamDestroyed;
            Session.Error += OnError;
            Session.Signal += OnSignal;
            Session.Connect(UserToken);

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
                        SubscriberKit.Connected -= OnSubscriberDidConnectToStream;
                        SubscriberKit.VideoDisabled -= OnSubscriberVideoDisabled;
                        SubscriberKit.VideoEnabled -= OnSubscriberVideoEnabled;
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
                        Session.Connected -= OnDidConnect;
                        Session.StreamReceived -= OnStreamCreated;
                        Session.StreamDropped -= OnStreamDestroyed;
                        Session.Error -= OnError;
                        Session.Signal -= OnSignal;
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

        public override Task<bool> SendMessageAsync(string message)
        {
            Session.SendSignal(string.Empty, message);
            return Task.FromResult(true);
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

        public override void CycleCamera() => (PublisherKit as Publisher)?.CycleCamera();

        private void OnConnectionDestroyed(object sender, Session.ConnectionDestroyedEventArgs e) => EndSession();

        private void OnDidConnect(object sender, EventArgs e)
        {
            lock (_locker)
            {
                if (Session == null ||
                    PublisherKit != null)
                {
                    return;
                }

                PublisherKit = new Publisher.Builder(CrossCurrentActivity.Current.AppContext).Build();
                PublisherKit.StreamCreated += OnPublisherStreamCreated;

                PublisherKit.SetStyle(BaseVideoRenderer.StyleVideoScale, BaseVideoRenderer.StyleVideoFill);
                PublisherKit.PublishAudio = IsAudioPublishingEnabled;
                PublisherKit.PublishVideo = IsVideoPublishingEnabled;
                PublisherUpdated?.Invoke();
                Session.Publish(PublisherKit);
                PublisherKit.PublishVideo = IsVideoPublishingEnabled;
            }
        }

        private void OnStreamCreated(object sender, Session.StreamReceivedEventArgs e)
        {
            lock (_locker)
            {
                if (Session == null ||
                    SubscriberKit != null)
                {
                    return;
                }

                SubscriberKit = new Subscriber.Builder(CrossCurrentActivity.Current.AppContext, e.P1).Build();
                SubscriberKit.Connected += OnSubscriberDidConnectToStream;
                SubscriberKit.VideoDisabled += OnSubscriberVideoDisabled;
                SubscriberKit.VideoEnabled += OnSubscriberVideoEnabled;

                SubscriberKit.SetStyle(BaseVideoRenderer.StyleVideoScale, BaseVideoRenderer.StyleVideoFill);
                SubscriberKit.SubscribeToAudio = IsAudioSubscriptionEnabled;
                SubscriberKit.SubscribeToVideo = IsVideoSubscriptionEnabled;
                SubscriberUpdated?.Invoke();
                Session.Subscribe(SubscriberKit);
            }
        }

        private void OnStreamDestroyed(object sender, Session.StreamDroppedEventArgs e) => SubscriberUpdated?.Invoke();

        private void OnError(object sender, Session.ErrorEventArgs e)
        {
            RaiseError(e.P1?.ErrorDomain?.Name());
            EndSession();
        }

        private void OnSubscriberVideoDisabled(object sender, Subscriber.VideoDisabledEventArgs e)
        => IsSubscriberVideoEnabled = false;

        private void OnSubscriberVideoEnabled(object sender, Subscriber.VideoEnabledEventArgs e)
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

        private void OnPublisherStreamCreated(object sender, PublisherKit.StreamCreatedEventArgs e)
        => IsPublishingStarted = true;

        private void OnSignal(object sender, Session.SignalEventArgs e)
        => RaiseMessageReceived(e.P2);
    }
}
