using Xamarin.Forms;
using Xamarin.Forms.OpenTok;
using Xamarin.Forms.OpenTok.iOS.Service;
using Xamarin.Forms.OpenTok.iOS;
using UIKit;

[assembly: ExportRenderer(typeof(OpenTokSubscriberView), typeof(OpenTokSubscriberViewRenderer))]
namespace Xamarin.Forms.OpenTok.iOS
{
    public class OpenTokSubscriberViewRenderer : OpenTokViewRenderer
    {
        protected override UIView GetNativeView() => PlatformOpenTokService.Instance.SubscriberKit?.View;

        protected override void SubscribeResetControl() => PlatformOpenTokService.Instance.SubscriberUpdated += ResetControl;

        protected override void UnsubscribeResetControl() => PlatformOpenTokService.Instance.ClearSubscribeUpdated();
    }
}
