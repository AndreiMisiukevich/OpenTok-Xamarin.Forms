using Xamarin.Forms.Platform.iOS;
using UIKit;

namespace Xamarin.Forms.OpenTok.iOS
{
    public abstract class OpenTokViewRenderer : ViewRenderer
    {
        protected OpenTokView OpenTokView => Element as OpenTokView;

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
            OpenTokView.IsVideoRunning = view != null;
            view = view ?? new UIView();

            if (Control == view)
            {
                return;
            }
            SetNativeControl(view);
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
            }
        }
    }
}