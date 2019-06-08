using Xamarin.Forms.OpenTok.Service;

namespace Xamarin.Forms.OpenTok.Sample
{
    public class StartPage : ContentPage
    {
        public StartPage()
        {
            Content = new StackLayout
            {
                Children =
                {
                    new Button
                    {
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        Text = "CLICK TO CHAT",
                        Command = new Command(() => {
                            if(!CrossOpenTok.Current.TryStartSession())
                            {
                                return;
                            }
                            Navigation.PushAsync(new ChatPage());
                        })
                    }
                }
            };
        }
    }
}