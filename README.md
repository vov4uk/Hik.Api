# Hik.Api
* Avaliable as [nuget](https://www.nuget.org/packages/Hik.Api/) 
* `dotnet add package Hik.Api --version 1.0.14`

* [![NuGet Downloads](https://img.shields.io/nuget/dt/Hik.Api.svg)](https://www.nuget.org/packages/Hik.Api/)

* Wrapper over Hikvision SDK version 5.3.6.30 x64. It allows login, fetch files list (videos and photos), download files, get config list and more.

* Or just run console app [sample](https://raw.githubusercontent.com/vov4uk/Hik.Api/main/sample/Program.cs)

Initialization
```cs
IHikApi hikApi = new HikApi();
hikApi.Initialize();
hikApi.SetupLogs(3, "C:\\sdkLogsPath", false);
hikApi.SetConnectTime(2000, 1);
hikApi.SetReconnect(10000, 1);
```

Login. Returns [Session](https://github.com/vov4uk/Hik.Api/blob/main/src/Hik.Api/Data/Session.cs)
```cs
var session = hikApi.Login("192.168.1.64", 8000, "admin", "pass");
```

> **Warning**:
> Almost every method requires **session.UserId**.
> I'm going to remove this parameter in future versions of package.

Logout
```cs
hikApi.Logout(session.UserId);
hikApi.Cleanup();
```

Print list of IP channels for NVR (IP Camera use session.Device.DefaultIpChannel)
```cs
foreach (var channel in session.Device.IpChannels)
{
    Console.WriteLine($"{channel.Name} {channel.ChannelNumber}; IsOnline : {channel.IsOnline};");
}
```

Get SD Card info, capaity, free space, status etc.
Returns [HdInfo](https://github.com/vov4uk/Hik.Api/blob/main/src/Hik.Api/Data/HdInfo.cs)
```cs
var info = hikApi.GetHddStatus(session.UserId);
Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(info));
```

Get device config.
Returns [DeviceConfig](https://github.com/vov4uk/Hik.Api/blob/main/src/Hik.Api/Data/DeviceConfig.cs)
```cs
var config = hikApi.GetDeviceConfig(session.UserId);
Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(config));
```

Get network config.
Returns [NetworkConfig](https://github.com/vov4uk/Hik.Api/blob/main/src/Hik.Api/Data/NetworkConfig.cs)
```cs
var network = hikApi.GetNetworkConfig(session.UserId);
Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(network));
```

Get device current time
```cs
var cameraTime = hikApi.GetTime(session.UserId);
Console.WriteLine($"Camera time :{cameraTime}");
```

Set device time
```cs
var currentTime = DateTime.Now;
hikApi.SetTime(currentTime, session.UserId);
```

# Photo service

Get photos list from IP Camera (default IP channel). Returns IReadOnlyCollection<[HikRemoteFile](https://github.com/vov4uk/Hik.Api/blob/main/src/Hik.Api/Data/HikRemoteFile.cs)>
```cs
//Get photos files for last 24 hours
DateTime fromPeriod = DateTime.Now.AddHours(-24);
DateTime toPeriod = DateTime.Now;
var photos = await hikApi.PhotoService.FindFilesAsync(fromPeriod, toPeriod, session);
```

Get photos list from specific IP channel.
```cs
int channel = 2;
var photos = await hikApi.PhotoService.FindFilesAsync(fromPeriod, toPeriod, session, channel);
```

Download photos
```cs
foreach (var photo in photos)
{
    hikApi.PhotoService.DownloadFile(
        session.UserId,
        photo.Name,
        photo.Size,
        photo.ToPhotoFileNameString());
}
```
or 
```cs
hikApi.PhotoService.DownloadFile(session.UserId, photo, photo.ToPhotoFileNameString());
```

# Video service
Get videos list from IP Camera (default IP channel). Returns IReadOnlyCollection<[HikRemoteFile](https://github.com/vov4uk/Hik.Api/blob/main/src/Hik.Api/Data/HikRemoteFile.cs)>
```cs
var videos = await hikApi.VideoService.FindFilesAsync(fromPeriod, toPeriod, session);
```

Get videos list from IP Camera (specific IP channel)
```cs
int channel = 2;
var videos = await hikApi.VideoService.FindFilesAsync(fromPeriod, toPeriod, session, channel);
```

Download video
```cs
foreach (var video in videos)
{
    Console.WriteLine($"Downloading {video.ToVideoFileNameString()}");
    var downloadId = hikApi.VideoService.StartDownloadFile(
        session.UserId,
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
var playbackId = hikApi.PlaybackService.StartPlayBack(session.UserId, channel);
```

or start live view to WinForm PictureBox 
```cs
int channel = 2;
System.Windows.Forms.PictureBox pctBox = new System.Windows.Forms.PictureBox();
var playbackId = hikApi.PlaybackService.StartPlayBack(session.UserId, channel, pctBox.Handle);
```

Start recording live stream to filePath in .mp4 format (need start playback first)
```cs
int channel = 2;
hikApi.PlaybackService.StartRecording(
    playbackId,
    "filePath.mp4",
    session.UserId,
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