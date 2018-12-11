using System;
using Xamarin.Forms.OpenTok.Service;
namespace Xamarin.Forms.OpenTok.Sample
{
    public class ChatPage : ContentPage
    {
        public ChatPage(double width, double height)
        {
            Content = new StackLayout
            {
                Children =
                {
                    new OpenTokPublisherView
                    {
                        WidthRequest = width,
                        HeightRequest = height / 2 - 40
                    },
                    new OpenTokSubscriberView
                    {
                        WidthRequest = width,
                        HeightRequest = height / 2 - 40
                    },
                    new Button
                    {
                        HeightRequest = 80,
                        BackgroundColor = Color.DarkRed,
                        TextColor = Color.White,
                        Text = "END CALL",
                        Command = new Command(() =>
                        {
                            CrossOpenTok.Current.EndSession();
                            Navigation.PopAsync();
                        })
                    }
                }
            };
        }
    }
}