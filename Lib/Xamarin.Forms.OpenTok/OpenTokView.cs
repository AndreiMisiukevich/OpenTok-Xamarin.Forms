namespace Xamarin.Forms.OpenTok
{
    public abstract class OpenTokView : View
    {
        public static BindableProperty IsVideoViewRunningProperty = BindableProperty.Create(nameof(IsVideoViewRunning), typeof(bool), typeof(OpenTokView), false, BindingMode.OneWayToSource);

        public bool IsVideoViewRunning
        {
            get => (bool)GetValue(IsVideoViewRunningProperty);
            set => SetValue(IsVideoViewRunningProperty, value);
        }

        public void SetIsVideoViewRunning(bool value) => IsVideoViewRunning = value;
    }
}