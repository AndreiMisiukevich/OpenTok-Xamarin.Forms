using Xamarin.Forms.Platform.Android;
using Android.Content;
using AView = Android.Views.View;
using Android.Runtime;

namespace Xamarin.Forms.OpenTok.Android
{
    [Preserve(AllMembers = true)]
    public abstract class OpenTokViewRenderer : ViewRenderer
    {
        private AView _defaultView;

        protected OpenTokViewRenderer(Context context) : base(context)
        {
        }

        protected OpenTokView OpenTokView => Element as OpenTokView;

        protected virtual AView DefaultView => _defaultView ?? (_defaultView = new AView(Context));

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            if (e.OldElement != null)
            {
                UnsubscribeResetControl();
            }
            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    ResetControl();
                }
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
                view.RemoveFromParent();
                SetNativeControl(view);
            }
        }

        protected abstract AView GetNativeView();

        protected abstract void SubscribeResetControl();

        protected abstract void UnsubscribeResetControl();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                using (_defaultView)
                {
                    UnsubscribeResetControl();
                }
            }
        }
    }
}