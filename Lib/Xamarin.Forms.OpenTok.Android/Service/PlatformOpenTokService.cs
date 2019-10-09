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
using Android.Util;
using System.Collections.ObjectModel;

namespace Xamarin.Forms.OpenTok.Android.Service
{
    [Preserve(AllMembers = true)]
    public sealed class PlatformOpenTokService : BaseOpenTokService
    {
        public event Action PublisherUpdated;
        public event Action SubscriberUpdated;

        private readonly string[] _requestPermissions = {
            Manifest.Permission.Camera,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.RecordAudio,
            Manifest.Permission.ModifyAudioSettings,
            Manifest.Permission.Internet,
            Manifest.Permission.AccessNetworkState
        };
        private readonly object _sessionLocker = new object();
        private readonly ObservableCollection<string> _subscriberStreamIds = new ObservableCollection<string>();
        private readonly Collection<SubscriberKit> _subscribers = new Collection<SubscriberKit>();

        private PlatformOpenTokService()
        {
            PropertyChanged += OnPropertyChanged;
            StreamIdCollection = new ReadOnlyObservableCollection<string>(_subscriberStreamIds);
            Subscribers = new ReadOnlyCollection<SubscriberKit>(_subscribers);
        }

        public static PlatformOpenTokService Instance => CrossOpenTok.Current as PlatformOpenTokService;

        public override ReadOnlyObservableCollection<string> StreamIdCollection { get; }
        public ReadOnlyCollection<SubscriberKit> Subscribers { get; }
        public Session Session { get; private set; }
        public PublisherKit PublisherKit { get; private set; }

        public static void Init()
        {
            OpenTokPublisherViewRenderer.Preserve();
            OpenTokSubscriberViewRenderer.Preserve();
            CrossOpenTok.Init(() => new PlatformOpenTokService());
        }

        public override bool TryStartSession()
        {
            lock (_sessionLocker)
            {
                if (!CheckPermissions() ||
                    string.IsNullOrWhiteSpace(ApiKey) ||
                    string.IsNullOrWhiteSpace(SessionId) ||
                    string.IsNullOrWhiteSpace(UserToken))
                {
                    return false;
                }

                EndSession();
                IsSessionStarted = true;

                using (var builder = new Session.Builder(CrossCurrentActivity.Current.AppContext, ApiKey, SessionId))
                {
                    Session = builder.Build();
                    Session.ConnectionDestroyed += OnConnectionDestroyed;
                    Session.Connected += OnConnected;
                    Session.StreamReceived += OnStreamReceived;
                    Session.StreamDropped += OnStreamDropped;
                    Session.Error += OnError;
                    Session.Signal += OnSignal;
                    Session.Connect(UserToken);
                }
                return true;
            }
        }

        public override void EndSession()
        {
            lock (_sessionLocker)
            {
                try
                {
                    if (Session == null)
                    {
                        return;
                    }

                    foreach (var subscriberKit in _subscribers)
                    {
                        ClearSubscriber(subscriberKit);
                    }
                    _subscribers.Clear();
                    _subscriberStreamIds.Clear();

                    if (PublisherKit != null)
                    {
                        using (PublisherKit)
                        {
                            PublisherKit.PublishAudio = false;
                            PublisherKit.PublishVideo = false;
                            PublisherKit.StreamCreated -= OnPublisherStreamCreated;
                        }
                        PublisherKit = null;
                    }

                    RaisePublisherUpdated().
                        RaiseSubscriberUpdated();

                    if (Session != null)
                    {
                        using (Session)
                        {
                            Session.ConnectionDestroyed -= OnConnectionDestroyed;
                            Session.Connected -= OnConnected;
                            Session.StreamReceived -= OnStreamReceived;
                            Session.StreamDropped -= OnStreamDropped;
                            Session.Error -= OnError;
                            Session.Signal -= OnSignal;
                            Session.Disconnect();
                        }
                        Session = null;
                    }

                }
                finally
                {
                    IsSessionStarted = false;
                    IsPublishingStarted = false;
                }
            }
        }

        public override bool CheckPermissions()
        {
            var activity = CrossCurrentActivity.Current.Activity;
            var shouldGrantPermissions = _requestPermissions.Any(permission => ContextCompat.CheckSelfPermission(activity, permission) != (int)Permission.Granted);
            if (shouldGrantPermissions)
            {
                ActivityCompat.RequestPermissions(activity, _requestPermissions, 0);
            }
            return !shouldGrantPermissions;
        }

        public override Task<bool> SendMessageAsync(string message)
        {
            Session.SendSignal(string.Empty, message);
            return Task.FromResult(true);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsVideoPublishingEnabled):
                    UpdatePublisherProperty(p => p.PublishVideo = IsVideoPublishingEnabled);
                    return;
                case nameof(IsAudioPublishingEnabled):
                    UpdatePublisherProperty(p => p.PublishAudio = IsAudioPublishingEnabled);
                    return;
                case nameof(IsVideoSubscriptionEnabled):
                    UpdateSubscriberProperty(s => s.SubscribeToVideo = IsVideoSubscriptionEnabled);
                    return;
                case nameof(IsAudioSubscriptionEnabled):
                    UpdateSubscriberProperty(s => s.SubscribeToAudio = IsAudioSubscriptionEnabled);
                    return;
            }
        }

        private void UpdatePublisherProperty(Action<PublisherKit> updateAction)
        {
            if (PublisherKit == null)
            {
                return;
            }
            updateAction?.Invoke(PublisherKit);
        }

        private void UpdateSubscriberProperty(Action<SubscriberKit> updateAction)
        {
            foreach (var subscriberKit in _subscribers)
            {
                updateAction?.Invoke(subscriberKit);
            }
        }

        public override void CycleCamera() => (PublisherKit as Publisher)?.CycleCamera();

        private void OnConnectionDestroyed(object sender, Session.ConnectionDestroyedEventArgs e)
            => RaiseSubscriberUpdated();
        
        private void OnConnected(object sender, Session.ConnectedEventArgs e)
        {
            if (Session == null || PublisherKit != null)
            {
                return;
            }

            using (var builder = new Publisher.Builder(CrossCurrentActivity.Current.AppContext).Name("XamarinOpenTok"))
            {
                PublisherKit = builder.Build();
                PublisherKit.PublishVideo = IsVideoPublishingEnabled;
                PublisherKit.PublishAudio = IsAudioPublishingEnabled;
                PublisherKit.SetStyle(BaseVideoRenderer.StyleVideoScale, BaseVideoRenderer.StyleVideoFill);
                PublisherKit.StreamCreated += OnPublisherStreamCreated;

                Session.Publish(PublisherKit);
                RaisePublisherUpdated();
            }
        }

        private void OnStreamReceived(object sender, Session.StreamReceivedEventArgs e)
        {
            if (Session == null || _subscribers.Any(x => x.Stream?.StreamId == e.P1?.StreamId))
            {
                return;
            }

            using (var builder = new Subscriber.Builder(CrossCurrentActivity.Current.AppContext, e.P1))
            {
                var subscriberKit = builder.Build();
                subscriberKit.SubscribeToAudio = IsAudioSubscriptionEnabled;
                subscriberKit.SubscribeToVideo = IsVideoSubscriptionEnabled;
                subscriberKit.SetStyle(BaseVideoRenderer.StyleVideoScale, BaseVideoRenderer.StyleVideoFill);

                subscriberKit.Connected += OnSubscriberConnected;
                subscriberKit.StreamDisconnected += OnStreamDisconnected;
                subscriberKit.SubscriberDisconnected += OnSubscriberDisconnected;
                subscriberKit.VideoDataReceived += OnSubscriberVideoDataReceived;
                subscriberKit.VideoDisabled += OnSubscriberVideoDisabled;
                subscriberKit.VideoEnabled += OnSubscriberVideoEnabled;

                Session.Subscribe(subscriberKit);
                var streamId = e.P1.StreamId;
                _subscribers.Add(subscriberKit);
                _subscriberStreamIds.Add(streamId);
            }
        }

        private void OnStreamDropped(object sender, Session.StreamDroppedEventArgs e)
        {
            var streamId = e.P1.StreamId;
            var subscriberKit = _subscribers.FirstOrDefault(x => x.Stream?.StreamId == streamId);
            if (subscriberKit != null)
            {
                ClearSubscriber(subscriberKit);
                _subscribers.Remove(subscriberKit);
            }
            _subscriberStreamIds.Remove(streamId);
            RaiseSubscriberUpdated();
        }

        private void OnError(object sender, Session.ErrorEventArgs e)
        {
            RaiseError(e.P1?.Message);
            EndSession();
        }

        private void OnSubscriberVideoDisabled(object sender, SubscriberKit.VideoDisabledEventArgs e)
            => RaiseSubscriberUpdated();

        private void OnSubscriberVideoDataReceived(object sender, SubscriberKit.VideoDataReceivedEventArgs e)
            => RaiseSubscriberUpdated();

        private void OnSubscriberVideoEnabled(object sender, SubscriberKit.VideoEnabledEventArgs e)
            => RaiseSubscriberUpdated();

        private void OnSubscriberConnected(object sender, SubscriberKit.ConnectedEventArgs e)
            => RaisePublisherUpdated().RaiseSubscriberUpdated();

        private void OnSubscriberDisconnected(object sender, SubscriberKit.SubscriberListenerDisconnectedEventArgs e)
            => RaisePublisherUpdated().RaiseSubscriberUpdated();

        private void OnStreamDisconnected(object sender, SubscriberKit.StreamListenerDisconnectedEventArgs e)
            => RaisePublisherUpdated().RaiseSubscriberUpdated();

        private PlatformOpenTokService RaiseSubscriberUpdated()
        {
            SubscriberUpdated?.Invoke();
            return this;
        }

        private PlatformOpenTokService RaisePublisherUpdated()
        {
            PublisherUpdated?.Invoke();
            return this;
        }

        private void OnPublisherStreamCreated(object sender, PublisherKit.StreamCreatedEventArgs e)
            => IsPublishingStarted = true;

        private void OnSignal(object sender, Session.SignalEventArgs e)
            => RaiseMessageReceived(e.P2);

        private void ClearSubscriber(SubscriberKit subscriberKit)
        {
            using (subscriberKit)
            {
                subscriberKit.SubscribeToAudio = false;
                subscriberKit.SubscribeToVideo = false;
                subscriberKit.Connected -= OnSubscriberConnected;
                subscriberKit.StreamDisconnected -= OnStreamDisconnected;
                subscriberKit.SubscriberDisconnected -= OnSubscriberDisconnected;
                subscriberKit.VideoDataReceived -= OnSubscriberVideoDataReceived;
                subscriberKit.VideoDisabled -= OnSubscriberVideoDisabled;
                subscriberKit.VideoEnabled -= OnSubscriberVideoEnabled;
            }
        }
    }
}
