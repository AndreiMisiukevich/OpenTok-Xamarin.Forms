using Xamarin.Forms.Platform.Android;
using Android.Content;
using AView = Android.Views.View;

namespace Xamarin.Forms.OpenTok.Android
{
    public abstract class OpenTokViewRenderer : ViewRenderer
    {
        private AView _defaultView;

        public OpenTokViewRenderer(Context context) : base(context)
        {
        }

        protected OpenTokView OpenTokView => Element as OpenTokView;

        protected AView DefaultView => _defaultView ?? (_defaultView = new AView(Context));

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

        protected abstract AView GetNativeView();

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