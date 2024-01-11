# Hik.Api
* Available as [nuget](https://www.nuget.org/packages/Hik.Api/) 
* `dotnet add package Hik.Api`

* [![NuGet Downloads](https://img.shields.io/nuget/dt/Hik.Api.svg)](https://www.nuget.org/packages/Hik.Api/)

* Wrapper over Hikvision SDK version 5.3.6.30 x64. It allows login, fetch files list (videos and photos), download files, get config list and more.

* Or just run console app [sample](https://raw.githubusercontent.com/vov4uk/Hik.Api/main/sample/Program.cs)

Initialization (static)
```cs
HikApi.Initialize();
```

Login (static). Returns [HikApi](https://github.com/vov4uk/Hik.Api/blob/main/src/Hik.Api/Hik.Api.cs)
```cs
var hikApi = HikApi.Login("192.168.1.64", 8000, "admin", "password");
```

Logout
```cs
hikApi.Logout();
```

Cleanup (static)
```cs
HikApi.Cleanup();
```

Print list of IP channels for NVR (IP Camera use session.Device.DefaultIpChannel)
```cs
foreach (var channel in hikApi.IpChannels)
{
    Console.WriteLine($"{channel.Name} {channel.ChannelNumber}; IsOnline : {channel.IsOnline};");
}
```

Get SD Card info, capacity, free space, status etc.
Returns [HdInfo](https://github.com/vov4uk/Hik.Api/blob/main/src/Hik.Api/Data/HdInfo.cs)
```cs
var info = hikApi.ConfigService.GetHddStatus();
Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(info));
```

Get device config.
Returns [DeviceConfig](https://github.com/vov4uk/Hik.Api/blob/main/src/Hik.Api/Data/DeviceConfig.cs)
```cs
var device = hikApi.ConfigService.GetDeviceConfig();
Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(config));
```

Get network config.
Returns [NetworkConfig](https://github.com/vov4uk/Hik.Api/blob/main/src/Hik.Api/Data/NetworkConfig.cs)
```cs
var network = hikApi.ConfigService.GetNetworkConfig();
Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(network));
```

Get device current time
```cs
var cameraTime = hikApi.ConfigService.GetTime();
Console.WriteLine($"Camera time :{cameraTime}");
```

Set device time
```cs
var currentTime = DateTime.Now;
hikApi.ConfigService.SetTime(currentTime);
```

# Photo service

Get photos list from IP Camera (default IP channel). Returns IReadOnlyCollection<[HikRemoteFile](https://github.com/vov4uk/Hik.Api/blob/main/src/Hik.Api/Data/HikRemoteFile.cs)>
```cs
//Get photos files for last 24 hours
DateTime fromPeriod = DateTime.Now.AddHours(-24);
DateTime toPeriod = DateTime.Now;
var photos = await hikApi.PhotoService.FindFilesAsync(fromPeriod, toPeriod);
```

Get photos list from specific IP channel.
```cs
int channel = 2;
var photos = await hikApi.PhotoService.FindFilesAsync(fromPeriod, toPeriod, channel);
```

Download photos
```cs
foreach (var photo in photos)
{
    hikApi.PhotoService.DownloadFile(
        photo.Name,
        photo.Size,
        photo.ToPhotoFileNameString());
}
```
or 
```cs
hikApi.PhotoService.DownloadFile(photo, photo.ToPhotoFileNameString());
```

# Video service
Get videos list from IP Camera (default IP channel). Returns IReadOnlyCollection<[HikRemoteFile](https://github.com/vov4uk/Hik.Api/blob/main/src/Hik.Api/Data/HikRemoteFile.cs)>
```cs
var videos = await hikApi.VideoService.FindFilesAsync(fromPeriod, toPeriod);
```

Get videos list from IP Camera (specific IP channel)
```cs
int channel = 2;
var videos = await hikApi.VideoService.FindFilesAsync(fromPeriod, toPeriod, channel);
```

Download video
```cs
foreach (var video in videos)
{
    Console.WriteLine($"Downloading {video.ToVideoFileNameString()}");
    var downloadId = hikApi.VideoService.StartDownloadFile(
        video.Name,
        video.ToVideoFileNameString());
    do
    {
        await Task.Delay(5000); // check progress every 5 sec
        int progress = hikApi.VideoService.GetDownloadPosition(downloadId);
        if (progress == 100)
        {
            hikApi.VideoService.StopDownloadFile(downloadId);
            break;
        }
        else if (progress < 0 || progress > 100)
        {
            throw new Exception($"Get progress failed, value = {downloadProgress}");
        }
    }
    while (true);
}
```

# Playback service
Start live preview without callback
```cs
int channel = 2;
var playbackId = hikApi.PlaybackService.StartPlayBack(channel);
```

or start live view to WinForm PictureBox 
```cs
int channel = 2;
System.Windows.Forms.PictureBox pctBox = new System.Windows.Forms.PictureBox();
var playbackId = hikApi.PlaybackService.StartPlayBack(channel, pctBox.Handle);
```

Start recording live stream to filePath in .mp4 format (need start playback first)
```cs
int channel = 2;
hikApi.PlaybackService.StartRecording(
    playbackId,
    "filePath.mp4",
    channel);
```

Stop recording live stream to filePath
```cs
hikApi.PlaybackService.StopRecording(playbackId);
```

Stop real play
```cs
hikApi.PlaybackService.StopPlayBack(playbackId);
```