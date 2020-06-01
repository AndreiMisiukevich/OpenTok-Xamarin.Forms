using Xamarin.Forms;
using Xamarin.Forms.OpenTok;
using Xamarin.Forms.OpenTok.Android;
using Android.Content;
using AView = Android.Views.View;
using Xamarin.Forms.OpenTok.Android.Service;
using Android.Runtime;
using SystemIntPtr = System.IntPtr;
using AndroidRuntimeJniHandleOwnership = Android.Runtime.JniHandleOwnership;
using Plugin.CurrentActivity;

[assembly: ExportRenderer(typeof(OpenTokPublisherView), typeof(OpenTokPublisherViewRenderer))]
namespace Xamarin.Forms.OpenTok.Android
{
    [Preserve(AllMembers = true)]
    public class OpenTokPublisherViewRenderer : OpenTokViewRenderer
    {
        public OpenTokPublisherViewRenderer(Context context) : base(context)
        {
        }

#pragma warning disable
        public OpenTokPublisherViewRenderer(SystemIntPtr p0, AndroidRuntimeJniHandleOwnership p1) : this(CrossCurrentActivity.Current.Activity)
        {
        }
#pragma warning restore

        public static void Preserve() { }

        protected override AView GetNativeView() => PlatformOpenTokService.Instance.PublisherKit?.View;

        protected override void SubscribeResetControl() => PlatformOpenTokService.Instance.PublisherUpdated += ResetControl;

        protected override void UnsubscribeResetControl() => PlatformOpenTokService.Instance.PublisherUpdated -= ResetControl;
    }
}
