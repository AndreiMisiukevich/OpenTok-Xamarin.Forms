using Xamarin.Forms;
using Xamarin.Forms.OpenTok;
using Xamarin.Forms.OpenTok.iOS.Service;
using Xamarin.Forms.OpenTok.iOS;
using UIKit;

[assembly: ExportRenderer(typeof(OpenTokPublisherView), typeof(OpenTokPublisherViewRenderer))]
namespace Xamarin.Forms.OpenTok.iOS
{
    public class OpenTokPublisherViewRenderer : OpenTokViewRenderer
    {
        protected override UIView GetNativeView() => PlatformOpenTokService.Instance.PublisherKit?.View;

        protected override void SubscribeResetControl() => PlatformOpenTokService.Instance.PublisherUpdated += ResetControl;

        protected override void UnsubscribeResetControl() => PlatformOpenTokService.Instance.ClearPublisherUpdated();
    }
}
