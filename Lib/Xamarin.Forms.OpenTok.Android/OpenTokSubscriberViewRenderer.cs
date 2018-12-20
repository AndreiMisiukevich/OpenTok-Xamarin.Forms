using Xamarin.Forms;
using Xamarin.Forms.OpenTok;
using Xamarin.Forms.OpenTok.Android;
using Android.Content;
using AView = Android.Views.View;
using Xamarin.Forms.OpenTok.Android.Service;

[assembly: ExportRenderer(typeof(OpenTokSubscriberView), typeof(OpenTokSubscriberViewRenderer))]
namespace Xamarin.Forms.OpenTok.Android
{
    public class OpenTokSubscriberViewRenderer : OpenTokViewRenderer
    {
        public OpenTokSubscriberViewRenderer(Context context) : base(context)
        {
        }

        protected override AView GetNativeView() => PlatformOpenTokService.Instance.SubscriberKit?.View;

        protected override void SubscribeResetControl() => PlatformOpenTokService.Instance.SubscriberUpdated += ResetControl;

        protected override void UnsubscribeResetControl() => PlatformOpenTokService.Instance.ClearSubscribeUpdated();
    }
}