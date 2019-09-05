using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.OpenTok;
using Xamarin.Forms.OpenTok.Android;
using Android.Content;
using AView = Android.Views.View;
using Xamarin.Forms.OpenTok.Android.Service;
using Android.Runtime;
using Com.Opentok.Android;

[assembly: ExportRenderer(typeof(OpenTokSubscriberView), typeof(OpenTokSubscriberViewRenderer))]
namespace Xamarin.Forms.OpenTok.Android
{
    [Preserve(AllMembers = true)]
    public class OpenTokSubscriberViewRenderer : OpenTokViewRenderer
    {
        public OpenTokSubscriberViewRenderer(Context context) : base(context)
        {
        }

        public static void Preserve() { }

        protected override AView GetNativeView(string streamId)
        {
            Dictionary<string, SubscriberKit> subscribers = PlatformOpenTokService.Instance.Subscribers;
            return subscribers.Count > 0 ? subscribers[streamId].View : null;
        }

        protected override void SubscribeResetControl() => PlatformOpenTokService.Instance.SubscriberUpdated += ResetControl;

        protected override void UnsubscribeResetControl() => PlatformOpenTokService.Instance.ClearSubscribeUpdated();
    }
}