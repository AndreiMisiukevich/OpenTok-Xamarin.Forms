using Xamarin.Forms;
using Xamarin.Forms.OpenTok;
using Xamarin.Forms.OpenTok.iOS.Service;
using System;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.OpenTok.iOS;
using UIKit;

[assembly: ExportRenderer(typeof(OpenTokSubscriberView), typeof(OpenTokSubscriberViewRenderer))]
namespace Xamarin.Forms.OpenTok.iOS
{
    public class OpenTokSubscriberViewRenderer : ViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            if (Control == null)
            {
                SetControl();
            }
            if (e.OldElement != null)
            {
                PlatformOpenTokService.Instance.ClearSubscribeUpdated();
            }
            if (e.NewElement != null)
            {
                PlatformOpenTokService.Instance.SubscriberUpdated += OnSubscriberUpdated;
            }
            base.OnElementChanged(e);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                PlatformOpenTokService.Instance.ClearSubscribeUpdated();
            }
        }

        private void OnSubscriberUpdated(object sender, EventArgs e) => SetControl();

        private void SetControl()
        {
            var view = PlatformOpenTokService.Instance.SubscriberKit?.View ?? new UIView();
            if (Control == view)
            {
                return;
            }
            SetNativeControl(view);
        }
    }
}
