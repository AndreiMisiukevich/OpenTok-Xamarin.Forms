# OpenTok for Xamarin.Forms

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


|Platform|Version|
| ------------------- | ------------------- |
|Xamarin.iOS|8.0+|
|Xamarin.Android|15+|

## Samples
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
```

Use **OpenTokPublisherView** and **OpenTokSubscriberView** for showing video from your camera and for recieving video from another chat participant. Just put them to any laouyt as you wish. When session was started, they would recieve video.


## License
The MIT License (MIT) see [License file](LICENSE)

## Contribution
Feel free to create issues and PRs 😃

