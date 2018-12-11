using Xamarin.Forms;
using Xamarin.Forms.OpenTok;
using Xamarin.Forms.OpenTok.Android;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using AView = Android.Views.View;
using Xamarin.Forms.OpenTok.Android.Service;
using System;

[assembly: ExportRenderer(typeof(OpenTokPublisherView), typeof(OpenTokPublisherViewRenderer))]
namespace Xamarin.Forms.OpenTok.Android
{
    public class OpenTokPublisherViewRenderer : ViewRenderer
    {
        public OpenTokPublisherViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            if (Control == null)
            {
                SetControl();
            }
            if(e.OldElement != null)
            {
                PlatformOpenTokService.Instance.ClearPublisherUpdated();
            }
            if (e.NewElement != null)
            {
                PlatformOpenTokService.Instance.PublisherUpdated += OnPublisherUpdated;
            }
            base.OnElementChanged(e);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if(disposing)
            {
                PlatformOpenTokService.Instance.ClearPublisherUpdated();
            }
        }

        private void OnPublisherUpdated(object sender, EventArgs e) => SetControl();

        private void SetControl()
        {
            var view = PlatformOpenTokService.Instance.PublisherKit?.View ?? new AView(Context);
            if(Control == view)
            {
                return;
            }
            SetNativeControl(view);
        }
    }
}
