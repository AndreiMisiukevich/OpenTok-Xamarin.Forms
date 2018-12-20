using Xamarin.Forms.Platform.Android;
using Android.Content;
using AView = Android.Views.View;

namespace Xamarin.Forms.OpenTok.Android
{
    public abstract class OpenTokViewRenderer : ViewRenderer
    {
        public OpenTokViewRenderer(Context context) : base(context)
        {
        }

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
            view = view ?? new AView(Context);

            if (Control == view)
            {
                return;
            }
            SetNativeControl(view);
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