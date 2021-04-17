using Xamarin.Forms;
using Xamarin.Forms.OpenTok;
using Xamarin.Forms.OpenTok.iOS.Service;
using Xamarin.Forms.OpenTok.iOS;
using UIKit;
using Foundation;
using System.ComponentModel;
using System.Linq;

[assembly: ExportRenderer(typeof(OpenTokSubscriberView), typeof(OpenTokSubscriberViewRenderer))]
namespace Xamarin.Forms.OpenTok.iOS
{
    [Preserve(AllMembers = true)]
    public class OpenTokSubscriberViewRenderer : OpenTokViewRenderer
    {
        public static void Preserve() { }

        protected OpenTokSubscriberView OpenTokSubscriberView => OpenTokView as OpenTokSubscriberView;

        protected override UIView GetNativeView()
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
            switch(e.PropertyName)
            {
                case nameof(OpenTokSubscriberView.StreamId):
                    ResetControl();
                    break;
            }
        }
    }
}
