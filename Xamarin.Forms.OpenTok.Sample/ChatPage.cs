using System;
using Xamarin.Forms.OpenTok.Service;
using System.IO;
namespace Xamarin.Forms.OpenTok.Sample
{
    public class ChatPage : ContentPage
    {
        private readonly OpenTokView _publisherView = new OpenTokPublisherView();
        private readonly OpenTokView _subscriberView = new OpenTokSubscriberView();
        private readonly StackLayout _messagesList = new StackLayout { Spacing = 5, Rotation = 180 };
        private readonly string _userIdentifier = Path.GetRandomFileName();

        public ChatPage()
        {
            CrossOpenTok.Current.MessageReceived += OnMessageReceived;

            Content = new StackLayout
            {
                Spacing = 0,
                Children =
                {
                    new StackLayout
                    {
                        Spacing = 0,
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            _publisherView,
                            _subscriberView
                        }
                    },
                    new ScrollView {
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        Padding = new Thickness(15, 5),
                        Content = _messagesList
                    },
                    new StackLayout
                    {
                        Spacing = 0,
                        HeightRequest = 60,
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            new Button
                            {
                                CornerRadius = 0,
                                VerticalOptions = LayoutOptions.FillAndExpand,
                                WidthRequest = 90,
                                BackgroundColor = Color.DarkRed,
                                TextColor = Color.White,
                                Text = "END CALL",
                                Command = new Command(() =>
                                {
                                    CrossOpenTok.Current.EndSession();
                                    CrossOpenTok.Current.MessageReceived -= OnMessageReceived;
                                    Navigation.PopAsync();
                                })
                            },
                            new Button
                            {
                                CornerRadius = 0,
                                VerticalOptions = LayoutOptions.FillAndExpand,
                                WidthRequest = 110,
                                BackgroundColor = Color.DarkGray,
                                TextColor = Color.White,
                                Text = "RANDOM MSG",
                                Command = new Command(() =>
                                {
                                    CrossOpenTok.Current.SendMessageAsync(_userIdentifier);
                                })
                            },
                            new Button
                            {
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                VerticalOptions = LayoutOptions.FillAndExpand,
                                CornerRadius = 0,
                                BackgroundColor = Color.Green,
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

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            _publisherView.HeightRequest = width / 2;
            _subscriberView.HeightRequest = width / 2;
            _publisherView.WidthRequest = width / 2;
            _subscriberView.WidthRequest = width / 2;
        }

        private void OnMessageReceived(string message)
        {
            var nickName = message == _userIdentifier ? "YOU" : "INTERLOCUTOR";
            _messagesList.Children.Add(new Label
            {
                FontSize = 18,
                Rotation = -180,
                TextColor = Color.Black,
                Text = $"{nickName}: {Path.GetRandomFileName()}"
            });
            _messagesList.Children.Add(new BoxView
            {
                BackgroundColor = Color.DarkGray,
                HeightRequest = 1,
                HorizontalOptions = LayoutOptions.FillAndExpand
            });
        }
    }
}