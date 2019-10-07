using Xamarin.Forms.Platform.iOS;
using UIKit;
using Foundation;

namespace Xamarin.Forms.OpenTok.iOS
{
    [Preserve(AllMembers = true)]
    public abstract class OpenTokViewRenderer : ViewRenderer
    {
        private UIView _defaultView;

        protected OpenTokView OpenTokView => Element as OpenTokView;

        protected virtual UIView DefaultView => _defaultView ?? (_defaultView = new UIView());

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            if (Control == null)
            {
                ResetControl();
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

        protected void ResetControl()
        {
            var view = GetNativeView();
            OpenTokView?.SetIsVideoViewRunning(view != null);
            view = view ?? DefaultView;
            if (Control != view)
            {
                SetNativeControl(view);
            }
        }

        protected abstract UIView GetNativeView();

        protected abstract void SubscribeResetControl();

        protected abstract void UnsubscribeResetControl();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                UnsubscribeResetControl();
                _defaultView?.RemoveFromSuperview();
                _defaultView?.Dispose();
                _defaultView = null;
            }
        }
    }
}