using Xamarin.Forms.Platform.Android;
using Android.Content;
using AView = Android.Views.View;
using Android.Runtime;
using Xamarin.Forms.OpenTok.Android.Service;

namespace Xamarin.Forms.OpenTok.Android
{
    [Preserve(AllMembers = true)]
    public abstract class OpenTokViewRenderer : ViewRenderer
    {
        private AView _defaultView;
        private string _streamId;

        protected OpenTokViewRenderer(Context context) : base(context)
        {
        }

        private OpenTokView OpenTokView => Element as OpenTokView;

        private AView DefaultView => _defaultView ?? (_defaultView = new AView(Context));

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                UnsubscribeResetControl();
            }

            if (e.NewElement != null)
            {
                if (e.NewElement is OpenTokSubscriberView subscriberView)
                {
                    _streamId = subscriberView.StreamId;
                }

                if (Control == null)
                {
                    ResetControl(this, new OpenTokUserUpdatedEventArgs(_streamId));
                }

                SubscribeResetControl();
            }
        }

        protected void ResetControl(object sender, OpenTokUserUpdatedEventArgs e)
        {
            AView view = GetNativeView(e.StreamId);
            OpenTokView?.SetIsVideoViewRunning(view != null);
            view = view ?? DefaultView;
            if (Control != view)
            {
                //Must not put this check in the if statement parent or it will kill publisher views for some reason.
                if (Element is OpenTokSubscriberView && e.StreamId != _streamId)
                {
                    return;
                }

                SetNativeControl(view);
            }
        }

        protected abstract AView GetNativeView(string streamId);

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