namespace Xamarin.Forms.OpenTok
{
    public abstract class OpenTokView : View
    {
        public static BindableProperty IsVideoRunningProperty = BindableProperty.Create(nameof(IsVideoRunning), typeof(bool), typeof(OpenTokView), false, BindingMode.OneWayToSource);

        public bool IsVideoRunning
        {
            get => (bool)GetValue(IsVideoRunningProperty);
            set => SetValue(IsVideoRunningProperty, value);
        }
    }
}