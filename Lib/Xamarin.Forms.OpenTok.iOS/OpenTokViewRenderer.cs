using Xamarin.Forms.Platform.iOS;
using UIKit;
using Foundation;
using Xamarin.Forms.OpenTok.iOS.Service;

namespace Xamarin.Forms.OpenTok.iOS
{
    [Preserve(AllMembers = true)]
    public abstract class OpenTokViewRenderer : ViewRenderer
    {
        private UIView _defaultView;
        public string StreamId;

        protected OpenTokView OpenTokView => Element as OpenTokView;

        protected UIView DefaultView => _defaultView ?? (_defaultView = new UIView());

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            if (e.NewElement is OpenTokSubscriberView subscriberView)
            {
                StreamId = subscriberView.StreamId;
            }

            if (Control == null)
            {
                ResetControl(this, new OpenTokUserUpdatedEventArgs(StreamId));
            }
            if (e.OldElement != null)
            {
                UnsubscribeResetControl();
            }
            if (e.NewElement != null)
            {
                SubscribeResetControl();
            }
            base.OnElementChanged(e);
        }

        protected void ResetControl(object sender, OpenTokUserUpdatedEventArgs e)
        {
            UIView view = GetNativeView(e.StreamId);
            OpenTokView?.SetIsVideoViewRunning(view != null);
            view = view ?? DefaultView;
            if (Control != view)
            {
                //Must not put this check in the if statement parent or it will kill publisher views for some reason.
                if (Element is OpenTokSubscriberView && e.StreamId != StreamId)
                {
                    return;
                }
                SetNativeControl(view);
            }
        }

        protected abstract UIView GetNativeView(string streamId);

        protected abstract void SubscribeResetControl();

        protected abstract void UnsubscribeResetControl();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                UnsubscribeResetControl();
                _defaultView?.Dispose();
            }
        }
    }
}