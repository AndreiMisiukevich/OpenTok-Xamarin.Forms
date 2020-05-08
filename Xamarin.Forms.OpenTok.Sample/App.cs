using Xamarin.Forms.OpenTok.Service;
using System.Net.Http;
using Newtonsoft.Json;

namespace Xamarin.Forms.OpenTok.Sample
{
    public class App : Application
    {
        public App()
        {
            MainPage = new NavigationPage(new StartPage());
        }

        protected override void OnStart()
        {
            // SETUP PROPERTIES FROM YOUR ACCOUNT
            CrossOpenTok.Current.ApiKey = "{API_KEY}";
            CrossOpenTok.Current.UserToken = "{USER_TOKEN}";
            CrossOpenTok.Current.SessionId = "{SESSION_ID}";

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
