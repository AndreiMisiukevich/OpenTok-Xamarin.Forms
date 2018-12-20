using System;
using System.ComponentModel;

namespace Xamarin.Forms.OpenTok.Service
{
    public interface IOpenTokService : INotifyPropertyChanged
    {
        event Action<string> Error;

        bool IsVideoPublishingEnabled { get; set; }

        bool IsAudioPublishingEnabled { get; set; }

        bool IsVideoSubscriptionEnabled { get; set; }

        bool IsAudioSubscriptionEnabled { get; set; }

        bool IsSubscriberVideoEnabled { get; set; }

        string ApiKey { get; set; }

        string SessionId { get; set; }

        string UserToken { get; set; }

        bool IsSessionStarted { get; set; }

        bool IsPublishingStarted { get; set; }

        bool CheckPermissions();

        bool TryStartSession();

        void EndSession();

        void CycleCamera();
    }
}