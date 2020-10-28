using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace Xamarin.Forms.OpenTok.Service
{
    public interface IOpenTokService : INotifyPropertyChanged
    {
        event Action<string> Error;

        event Action<string> MessageReceived;

        event NotifyCollectionChangedEventHandler StreamIdCollectionChanged;

        ReadOnlyObservableCollection<string> StreamIdCollection { get; }

        OpenTokPermission Permissions { get; set; }

        bool IsVideoPublishingEnabled { get; set; }

        bool IsAudioPublishingEnabled { get; set; }

        bool IsVideoSubscriptionEnabled { get; set; }

        bool IsAudioSubscriptionEnabled { get; set; }

        string ApiKey { get; set; }

        string SessionId { get; set; }

        string UserToken { get; set; }

        bool IsSessionStarted { get; set; }

        bool IsPublishingStarted { get; set; }

        bool CheckPermissions();

        bool TryStartSession();

        void EndSession();

        void CycleCamera();

        Task<bool> SendMessageAsync(string message);
    }
}