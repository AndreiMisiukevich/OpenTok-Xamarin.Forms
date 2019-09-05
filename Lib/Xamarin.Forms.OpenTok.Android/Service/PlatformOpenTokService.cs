using System;
using System.Collections.Generic;
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

namespace Xamarin.Forms.OpenTok.Android.Service
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

        private const int RequestId = 0;
        private const string Tag = "OpenTok.Android.Service"; //23 character limit

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
            Subscribers = new Dictionary<string, SubscriberKit>();
        }

        public static PlatformOpenTokService Instance => CrossOpenTok.Current as PlatformOpenTokService;

        private Session Session { get; set; }
        public PublisherKit PublisherKit { get; private set; }
        public Dictionary<string, SubscriberKit> Subscribers { get; private set; }

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

            var builder = new Session.Builder(CrossCurrentActivity.Current.AppContext, ApiKey, SessionId);

            Session = builder.Build();
            Session.ConnectionDestroyed += OnConnectionDestroyed;
            Session.Connected += OnConnected;
            Session.StreamReceived += OnStreamReceived;
            Session.StreamDropped += OnStreamDropped;
            Session.Error += OnError;
            Session.Signal += OnSignal;
            Session.Connect(UserToken);

            builder.Dispose();

            Log.Debug(Tag, "Session Started");
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
                        foreach (SubscriberKit subscriberKit in Subscribers.Select(subscriber => subscriber.Value))
                        {
                            subscriberKit.SubscribeToAudio = false;
                            subscriberKit.SubscribeToVideo = false;
                            subscriberKit.Connected -= OnSubscriberConnected;
                            subscriberKit.StreamDisconnected -= OnStreamDisconnected;
                            subscriberKit.SubscriberDisconnected -= OnSubscriberDisconnected;
                            subscriberKit.VideoDisabled -= OnSubscriberVideoDisabled;
                            subscriberKit.VideoEnabled -= OnSubscriberVideoEnabled;
                            subscriberKit.Dispose();
                        }

                        Subscribers = new Dictionary<string, SubscriberKit>();
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
                        Session.Connected -= OnConnected;
                        Session.StreamReceived -= OnStreamReceived;
                        Session.StreamDropped -= OnStreamDropped;
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
            if(Subscribers?.Count > 0)
            {
                switch(e.PropertyName)
                {
                    case nameof(IsVideoSubscriptionEnabled):
                        //TODO: This is probably wrong.
                        foreach (SubscriberKit subscriberKit in Subscribers.Select(subscriber => subscriber.Value))
                        {
                            subscriberKit.SubscribeToVideo = IsVideoSubscriptionEnabled;
                        }

                        return;
                    case nameof(IsAudioSubscriptionEnabled):
                        //TODO: This is probably wrong.
                        foreach (SubscriberKit subscriberKit in Subscribers.Select(subscriber => subscriber.Value))
                        {
                            subscriberKit.SubscribeToAudio = IsAudioSubscriptionEnabled;
                        }

                        return;
                }
            }
        }

        public override void CycleCamera() => (PublisherKit as Publisher)?.CycleCamera();

        private void OnConnectionDestroyed(object sender, Session.ConnectionDestroyedEventArgs e) => EndSession();

        private void OnConnected(object sender, Session.ConnectedEventArgs e)
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

                Session.Publish(PublisherKit);
                PublisherUpdated?.Invoke(sender, new OpenTokUserUpdatedEventArgs(PublisherKit.Session.SessionId));
            }
        }

        private void OnStreamReceived(object sender, Session.StreamReceivedEventArgs e)
        {
            lock (_locker)
            {
                if (Session == null || Subscribers != null &&
                        Subscribers.ContainsKey(e.P1.StreamId))
                {
                    return;
                }

                Stream stream = e.P1;
                var builder = new Subscriber.Builder(CrossCurrentActivity.Current.AppContext, stream);

                SubscriberKit subscriberKit = builder.Build();
                subscriberKit.Connected += OnSubscriberConnected;
                subscriberKit.StreamDisconnected += OnStreamDisconnected;
                subscriberKit.SubscriberDisconnected += OnSubscriberDisconnected;
                subscriberKit.VideoDisabled += OnSubscriberVideoDisabled;
                subscriberKit.VideoEnabled += OnSubscriberVideoEnabled;

                subscriberKit.SetStyle(BaseVideoRenderer.StyleVideoScale, BaseVideoRenderer.StyleVideoFill);
                subscriberKit.SubscribeToAudio = IsAudioSubscriptionEnabled;
                subscriberKit.SubscribeToVideo = IsVideoSubscriptionEnabled;
                Session.Subscribe(subscriberKit);

                Subscribers?.Add(stream.StreamId, subscriberKit);
                OnSubscriberAdded(stream.StreamId);

                builder.Dispose();
            }
        }

        private void OnStreamDropped(object sender, Session.StreamDroppedEventArgs e) => SubscriberUpdated?.Invoke(sender, new OpenTokUserUpdatedEventArgs(e.P1.StreamId));

        private void OnError(object sender, Session.ErrorEventArgs e)
        {
            RaiseError(e.P1?.ErrorDomain?.Name());
            EndSession();
        }

        private void OnSubscriberVideoDisabled(object sender, SubscriberKit.VideoDisabledEventArgs e)
        => IsSubscriberVideoEnabled = false;

        private void OnSubscriberVideoEnabled(object sender, SubscriberKit.VideoEnabledEventArgs e)
        {
            lock (_locker)
            {
                IsSubscriberVideoEnabled = e.P0?.Stream?.HasVideo ?? false;
            }
        }

        private void OnSubscriberConnected(object sender, SubscriberKit.ConnectedEventArgs e) => OnSubscriberConnectionChanged(true, e.P0);

        private void OnStreamDisconnected(object sender, SubscriberKit.StreamListenerDisconnectedEventArgs e) => OnSubscriberConnectionChanged(false, e.P0);

        private void OnSubscriberDisconnected(object sender, SubscriberKit.SubscriberListenerDisconnectedEventArgs e) => OnSubscriberConnectionChanged(false, e.P0);

        private void OnSubscriberConnectionChanged(bool isConnected, SubscriberKit subscriberKit)
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

        private void OnPublisherStreamCreated(object sender, PublisherKit.StreamCreatedEventArgs e)
            => IsPublishingStarted = true;

        private void OnSignal(object sender, Session.SignalEventArgs e)
            => RaiseMessageReceived(e.P2);

        private void OnSubscriberAdded(string streamId)
            => RaiseSubscriberAdded(streamId);
    }
}
