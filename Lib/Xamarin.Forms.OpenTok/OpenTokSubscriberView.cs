namespace Xamarin.Forms.OpenTok
{
    public sealed class OpenTokSubscriberView : OpenTokView
    {
        public string StreamId { get; set; }
        public OpenTokSubscriberView(string streamId)
        {
            StreamId = streamId;
        }
    }
}
