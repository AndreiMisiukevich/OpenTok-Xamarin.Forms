using Xamarin.Forms;
using Xamarin.Forms.OpenTok;
using Xamarin.Forms.OpenTok.iOS.Service;
using System;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms.OpenTok.iOS;
using UIKit;

[assembly: ExportRenderer(typeof(OpenTokPublisherView), typeof(OpenTokPublisherViewRenderer))]
namespace Xamarin.Forms.OpenTok.iOS
{
    public class OpenTokPublisherViewRenderer : ViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            if (Control == null)
            {
                SetControl();
            }
            if (e.OldElement != null)
            {
                PlatformOpenTokService.Instance.ClearPublisherUpdated();
            }
            if (e.NewElement != null)
            {
                PlatformOpenTokService.Instance.PublisherUpdated += OnPublisherUpdate;
            }
            base.OnElementChanged(e);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                PlatformOpenTokService.Instance.ClearPublisherUpdated();
            }
        }

        private void OnPublisherUpdate(object sender, EventArgs e) => SetControl();

        private void SetControl()
        {
            var view = PlatformOpenTokService.Instance.PublisherKit?.View ?? new UIView();
            if (Control == view)
            {
                return;
            }
            SetNativeControl(view);
        }
    }
}
