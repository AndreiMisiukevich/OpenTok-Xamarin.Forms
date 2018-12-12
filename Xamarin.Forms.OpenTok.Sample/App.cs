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

        protected override async void OnStart()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var resp = await client.GetAsync(Config.KeysUrl);
                    var json = await resp.Content.ReadAsStringAsync();
                    var keys = JsonConvert.DeserializeObject<Keys>(json);

                    CrossOpenTok.Current.ApiKey = keys.ApiKey;
                    CrossOpenTok.Current.SessionId = keys.SessionId;
                    CrossOpenTok.Current.UserToken = keys.Token;
                }
                catch
                {
                    await MainPage.DisplayAlert(null, "MAKE SURE YOU SET API URL FOR RETRIEVING NECESSARY KEYS (Config.cs) OR YOU MAY HARDCODE THEM.", "GOT IT");
                }
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
