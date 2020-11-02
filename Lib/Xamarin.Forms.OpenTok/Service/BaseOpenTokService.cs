using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xamarin.Forms.OpenTok.Service
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class BaseOpenTokService : IOpenTokService
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event Action<string> Error;

        public event Action<string> MessageReceived;

        public event NotifyCollectionChangedEventHandler StreamIdCollectionChanged;

        public abstract ReadOnlyObservableCollection<string> StreamIdCollection { get; }

        private readonly object _propertiesLocker = new object();

        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

        public OpenTokPermission Permissions
        {
            get => GetValue(OpenTokPermission.All);
            set => SetValue(value);
        }

        public OpenTokPublisherVideoType PublisherVideoType
        {
            get => GetValue(OpenTokPublisherVideoType.Camera);
            set => SetValue(value);
        }

        public bool IsVideoPublishingEnabled
        {
            get => GetValue(true);
            set => SetValue(value);
        }

        public bool IsAudioPublishingEnabled
        {
            get => GetValue(true);
            set => SetValue(value);
        }

        public bool IsVideoSubscriptionEnabled
        {
            get => GetValue(true);
            set => SetValue(value);
        }

        public bool IsAudioSubscriptionEnabled
        {
            get => GetValue(true);
            set => SetValue(value);
        }

        public string ApiKey
        {
            get => GetValue(string.Empty);
            set => SetValue(value);
        }

        public string SessionId
        {
            get => GetValue(string.Empty);
            set => SetValue(value);
        }

        public string UserToken
        {
            get => GetValue(string.Empty);
            set => SetValue(value);
        }

        public bool IsSessionStarted
        {
            get => GetValue(false);
            set => SetValue(value);
        }

        public bool IsPublishingStarted
        {
            get => GetValue(false);
            set => SetValue(value);
        }

        public abstract bool CheckPermissions();

        public abstract bool TryStartSession();

        public abstract void EndSession();

        public abstract void CycleCamera();

        public abstract Task<bool> SendMessageAsync(string message);

        protected void RaiseError(string message) => Error?.Invoke(message);

        protected void RaiseMessageReceived(string message) 
            => MessageReceived?.Invoke(message);

        protected void OnSubscriberStreamIdsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            => StreamIdCollectionChanged?.Invoke(sender, e);

        private T GetValue<T>(T defaultValue, [CallerMemberName] string name = null)
        {
            lock (_propertiesLocker)
            {
                return (T)(_properties.TryGetValue(name, out object value) ? value : defaultValue);
            }
        }

        private void SetValue<T>(T value, [CallerMemberName] string name = null)
        {
            lock (_propertiesLocker)
            {
                if (_properties.ContainsKey(name) && EqualityComparer<T>.Default.Equals((T)_properties[name], value))
                {
                    return;
                }
                _properties[name] = value;
            }
            RaisePropertyChanged(name);
        }

        private void RaisePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
