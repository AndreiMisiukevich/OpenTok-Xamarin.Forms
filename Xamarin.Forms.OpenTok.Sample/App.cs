using Xamarin.Forms.OpenTok.Service;
using System.Net.Http;
using Newtonsoft.Json;

namespace Xamarin.Forms.OpenTok.Sample
{
    public class App : Application
    {
        public App()
        {
            var publisher = new OpenTokPublisherView();
            AbsoluteLayout.SetLayoutFlags(publisher, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(publisher, new Rectangle(.5, .5, .5, .5));

            var subscriber = new OpenTokSubscriberView();
            AbsoluteLayout.SetLayoutFlags(subscriber, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(subscriber, new Rectangle(0, 1, 1, .1));

            MainPage = new NavigationPage(new StartPage());
        }

        protected override async void OnStart()
        {
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(Config.KeysUrl);
                var json = await resp.Content.ReadAsStringAsync();
                var keys = JsonConvert.DeserializeObject<Keys>(json);

                CrossOpenTok.Current.ApiKey = keys.ApiKey;
                CrossOpenTok.Current.SessionId = keys.SessionId;
                CrossOpenTok.Current.UserToken = keys.Token;
            }
            CrossOpenTok.Current.Error += (m) => MainPage.DisplayAlert("ERROR", m, "OK");
        }

        public sealed class Keys
        {
            public string ApiKey { get; set; }
            public string SessionId { get; set; }
            public string Token { get; set; }
        }
    }
}
