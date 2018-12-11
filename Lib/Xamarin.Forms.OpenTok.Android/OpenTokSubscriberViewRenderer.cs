using Xamarin.Forms;
using Xamarin.Forms.OpenTok;
using Xamarin.Forms.OpenTok.Android;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using AView = Android.Views.View;
using Xamarin.Forms.OpenTok.Android.Service;
using System;

[assembly: ExportRenderer(typeof(OpenTokSubscriberView), typeof(OpenTokSubscriberViewRenderer))]
namespace Xamarin.Forms.OpenTok.Android
{
    public class OpenTokSubscriberViewRenderer : ViewRenderer
        {
            public OpenTokSubscriberViewRenderer(Context context) : base(context)
            {
            }

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
                var view = PlatformOpenTokService.Instance.SubscriberKit?.View ?? new AView(Context);
                if (Control == view)
                {
                    return;
                }
                SetNativeControl(view);
            }
        }
    }
