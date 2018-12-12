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
                Spacing = 0,
                Children =
                {
                    new OpenTokPublisherView
                    {
                        WidthRequest = width,
                        HeightRequest = height / 2 - 30
                    },
                    new OpenTokSubscriberView
                    {
                        WidthRequest = width,
                        HeightRequest = height / 2 - 30
                    },
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 6,
                        Children =
                        {
                            new Button
                            {
                                CornerRadius = 0,
                                WidthRequest = width / 2 - 3,
                                HeightRequest = 60,
                                BackgroundColor = Color.DarkRed,
                                TextColor = Color.White,
                                Text = "END CALL",
                                Command = new Command(() =>
                                {
                                    CrossOpenTok.Current.EndSession();
                                    Navigation.PopAsync();
                                })
                            },
                            new Button
                            {
                                CornerRadius = 0,
                                WidthRequest = width / 2 - 3,
                                HeightRequest = 60,
                                BackgroundColor = Color.Gold,
                                TextColor = Color.White,
                                Text = "SWAP CAMERA",
                                Command = new Command(() =>
                                {
                                    CrossOpenTok.Current.CycleCamera();
                                })
                            }
                        }
                    }
                }
            };
        }
    }
}