using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.CurrentActivity;
using Xamarin.Forms.OpenTok.Android.Service;

namespace Xamarin.Forms.OpenTok.Sample.Droid
{
    [Activity(Label = "Xamarin.Forms.OpenTok.Sample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            PlatformOpenTokService.Init();
            base.OnCreate(savedInstanceState);

            CrossCurrentActivity.Current.Activity = this;
            Forms.Init(this, savedInstanceState);

            LoadApplication(new App());
        }
    }
}