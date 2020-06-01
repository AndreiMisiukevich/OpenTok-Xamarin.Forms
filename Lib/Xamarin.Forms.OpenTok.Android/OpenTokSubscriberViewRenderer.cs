using Xamarin.Forms;
using Xamarin.Forms.OpenTok;
using Xamarin.Forms.OpenTok.Android;
using Android.Content;
using AView = Android.Views.View;
using Xamarin.Forms.OpenTok.Android.Service;
using Android.Runtime;
using System.ComponentModel;
using System.Linq;
using SystemIntPtr = System.IntPtr;
using AndroidRuntimeJniHandleOwnership = Android.Runtime.JniHandleOwnership;
using Plugin.CurrentActivity;

[assembly: ExportRenderer(typeof(OpenTokSubscriberView), typeof(OpenTokSubscriberViewRenderer))]
namespace Xamarin.Forms.OpenTok.Android
{
    [Preserve(AllMembers = true)]
    public class OpenTokSubscriberViewRenderer : OpenTokViewRenderer
    {
        public OpenTokSubscriberViewRenderer(Context context) : base(context)
        {
        }

#pragma warning disable
        public OpenTokSubscriberViewRenderer(SystemIntPtr p0, AndroidRuntimeJniHandleOwnership p1) : this(CrossCurrentActivity.Current.Activity)
        {
        }
#pragma warning restore

        public static void Preserve() { }

        protected OpenTokSubscriberView OpenTokSubscriberView => OpenTokView as OpenTokSubscriberView;

        protected override AView GetNativeView()
        {
            var streamId = OpenTokSubscriberView?.StreamId;
            var subscribers = PlatformOpenTokService.Instance.Subscribers;
            return (streamId != null
                ? subscribers.FirstOrDefault(x => x.Stream?.StreamId == streamId)
                : subscribers.FirstOrDefault())?.View;
        }

        protected override void SubscribeResetControl() => PlatformOpenTokService.Instance.SubscriberUpdated += ResetControl;

        protected override void UnsubscribeResetControl() => PlatformOpenTokService.Instance.SubscriberUpdated -= ResetControl;

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            switch (e.PropertyName)
            {
                case nameof(OpenTokSubscriberView.StreamId):
                    ResetControl();
                    break;
            }
        }
    }
}