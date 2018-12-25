# OpenTok for Xamarin.Forms

## Setup
* Available on NuGet: [Xamarin.Forms.OpenTok](http://www.nuget.org/packages/Xamarin.Forms.OpenTok) [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.OpenTok.svg?label=NuGet)](https://www.nuget.org/packages/Xamarin.Forms.OpenTok)
* Add nuget package to your Xamarin.Forms .netStandard/PCL project and to your platform-specific projects
* Call **PlatformOpenTokService.Init()** in platform specific code (Typically it's **AppDelegate** for *iOS* and **MainActivity** for *Android*)



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

Then check wheather you have enough permissions for starting a call.
```csharp
if(!CrossOpenTok.Current.TryStartSession())
{
    return;
}
```

Use **OpenTokPublisherView** and **OpenTokSubscriberView** for showing video from your camera and for recieving video from another chat participant. Just put them to any laouyt as you wish. When session was started, they would start to recieve video.


## License
The MIT License (MIT) see [License file](LICENSE)

## Contribution
Feel free to create issues and PRs 😃

