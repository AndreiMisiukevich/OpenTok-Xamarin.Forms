using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.OpenTok;
using Xamarin.Forms.OpenTok.iOS.Service;
using Xamarin.Forms.OpenTok.iOS;
using UIKit;
using Foundation;
using OpenTok;

[assembly: ExportRenderer(typeof(OpenTokSubscriberView), typeof(OpenTokSubscriberViewRenderer))]
namespace Xamarin.Forms.OpenTok.iOS
{
    [Preserve(AllMembers = true)]
    public class OpenTokSubscriberViewRenderer : OpenTokViewRenderer
    {
        public static void Preserve() { }

        protected override UIView GetNativeView(string streamId)
        {
            Dictionary<string, OTSubscriber> subscribers = PlatformOpenTokService.Instance.Subscribers;
            return subscribers?.Count > 0 ? subscribers[streamId].View : null;
        }

        protected override void SubscribeResetControl() => PlatformOpenTokService.Instance.SubscriberUpdated += ResetControl;

        protected override void UnsubscribeResetControl() => PlatformOpenTokService.Instance.ClearSubscribeUpdated();
    }
}
