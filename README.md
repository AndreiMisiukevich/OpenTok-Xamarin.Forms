# OpenTok for Xamarin.Forms
https://tokbox.com/

## Setup
* Available on NuGet: [Xamarin.Forms.OpenTok](http://www.nuget.org/packages/Xamarin.Forms.OpenTok) [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.OpenTok.svg?label=NuGet)](https://www.nuget.org/packages/Xamarin.Forms.OpenTok)
* Add nuget package to your Xamarin.Forms .netStandard/PCL project and to your platform-specific projects
* Call **PlatformOpenTokService.Init()** in platform specific code (Typically it's **AppDelegate** for *iOS* and **MainActivity** for *Android*)

**iOS:**
Add messages for requesting permissions to Info.plist file
```xml
	<key>NSCameraUsageDescription</key>
	<string>use camera to start video call</string>
	<key>NSMicrophoneUsageDescription</key>
	<string>use microphone to start video call</string>
```

**Android:**
Add permissions to Manifest file
```xml
	<uses-permission android:name="android.permission.RECORD_AUDIO" />
	<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
	<uses-permission android:name="android.permission.CAMERA" />
	<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
	<uses-permission android:name="android.permission.INTERNET" />
	<uses-permission android:name="android.permission.MODIFY_AUDIO_SETTINGS" />
```
Setup CrossCurrentActivity plugin in MainActivity:

```csharp
public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        TabLayoutResource = Resource.Layout.Tabbar;
        ToolbarResource = Resource.Layout.Toolbar;
        PlatformOpenTokService.Init();
        base.OnCreate(savedInstanceState);
        
        // ==== Add this line ==== //
        CrossCurrentActivity.Current.Activity = this;
	// ==== ............. ==== //

        Forms.Init(this, savedInstanceState);
        LoadApplication(new App());
    }
}
```


|Platform|Version|
| ------------------- | ------------------- |
|Xamarin.iOS|8.0+|
|Xamarin.Android|15+|

## Samples

**SAMPLE VIDEO: https://twitter.com/Andrik_Just4Fun/status/1151799321223995392**

The sample you can find here: https://github.com/AndreiMisiukevich/OpenTok-Xamarin.Forms/tree/master/Xamarin.Forms.OpenTok.Sample

Use **CrossOpenTok.Current** for accessing OpenTok service.

Full api you can find here: https://github.com/AndreiMisiukevich/OpenTok-Xamarin.Forms/blob/master/Lib/Xamarin.Forms.OpenTok/Service/IOpenTokService.cs


Firstly you should setup your service
```csharp
CrossOpenTok.Current.ApiKey = keys.ApiKey; // OpenTok api key from your account
CrossOpenTok.Current.SessionId = keys.SessionId; // Id of session for connecting
CrossOpenTok.Current.UserToken = keys.Token; // User's token
```

Then check wheather you have enough permissions for starting a call and if everything is fine it will start a session.
```csharp
if(!CrossOpenTok.Current.TryStartSession())
{
    return;
}
//Session is starting, you may show Chat Page
```

Use **OpenTokPublisherView** and **OpenTokSubscriberView** for showing video from your camera and for recieving video from another chat participant. Just put them to any laouyt as you wish. When session was started, they would recieve video.

```xaml
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:tok="clr-namespace:Xamarin.Forms.OpenTok;assembly=Xamarin.Forms.OpenTok"
             x:Class="Xamarin.Forms.OpenTok.Sample.ChatRoomPage"
             BackgroundColor="White">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*" />
            <RowDefinition Height="*" />
            <RowDefinition Height="80" />
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>
        
        <tok:OpenTokSubscriberView Grid.Row="0" Grid.Column="0" Grid.ColumnSpan="3" />
        <tok:OpenTokPublisherView Grid.Row="1" Grid.Column="0" Grid.ColumnSpan="3" />
        
        <Button Text="End Call" TextColor="Red" Grid.Row="2" Grid.Column="0" Clicked="OnEndCall" />
        <Button Text="Message" TextColor="Black" Grid.Row="2" Grid.Column="1" Clicked="OnMessage" />
        <Button Text="Swap Camera" TextColor="Purple" Grid.Row="2" Grid.Column="2" Clicked="OnSwapCamera" />
        
    </Grid>
</ContentPage>
```
Check source code for more info, 🇧🇾 just ask me =)

## License
The MIT License (MIT) see [License file](LICENSE)

## Contribution
Feel free to create issues and PRs 😃

