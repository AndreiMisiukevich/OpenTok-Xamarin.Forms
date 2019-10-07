namespace Xamarin.Forms.OpenTok
{
    public sealed class OpenTokSubscriberView : OpenTokView
    {
        public static readonly BindableProperty StreamIdProperty = BindableProperty.Create(nameof(StreamId), typeof(string), typeof(OpenTokSubscriberView), null);

        public string StreamId
        {
            get => GetValue(StreamIdProperty) as string;
            set => SetValue(StreamIdProperty, value);
        }
    }
}