using Hik.Api;
using Hik.Api.Abstraction;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("App started");
                Directory.CreateDirectory("Videos");
                Directory.CreateDirectory("Photos");
                IHikApi hikApi = new HikApi();
                hikApi.Initialize();
                hikApi.SetupLogs(3, "C:\\sdkLogsPath", false);
                hikApi.SetConnectTime(2000, 1);
                hikApi.SetReconnect(10000, 1);

                // Please update IP Address, port and user credentials
                var session = hikApi.Login("192.168.1.64", 8000, "admin", "password");
                Console.WriteLine("Login success");

                // Get Camera time
                var cameraTime = hikApi.GetTime(session.UserId);
                Console.WriteLine($"Camera time :{cameraTime}");
                var currentTime = DateTime.Now;
                if (Math.Abs((currentTime - cameraTime).TotalSeconds) > 5)
                {
                    hikApi.SetTime(currentTime, session.UserId);
                }

                // GetNetworkConfig
                var network = hikApi.GetNetworkConfig(session.UserId);
                Console.WriteLine(JsonConvert.SerializeObject(network, Formatting.Indented));

                // GetDeviceConfig
                var device = hikApi.GetDeviceConfig(session.UserId);
                Console.WriteLine(JsonConvert.SerializeObject(device, Formatting.Indented));

                // For NVR
                if (session.Device.IpChannels.Any())
                {
                    Console.WriteLine($"Found {session.Device.IpChannels} IpChannels");
                    foreach (var channel in session.Device.IpChannels)
                    {
                        Console.WriteLine($"IP Channel {channel.ChannelNumber}; IsOnline : {channel.IsOnline};");
                        if (channel.IsOnline)
                        {
                            var videos = await hikApi.VideoService.FindFilesAsync(DateTime.Now.AddHours(-4), DateTime.Now, session, channel.ChannelNumber);
                            Console.WriteLine($"Found {videos.Count} videos");
                            foreach (var video in videos)
                            {
                                Console.WriteLine(video.Name);
                            }
                        }
                    }
                }
                else
                {
                    //Get photos files for last 2 hours
                    var photos = await hikApi.PhotoService.FindFilesAsync(DateTime.Now.AddHours(-2), DateTime.Now, session);
                    Console.WriteLine($"Found {photos.Count} photos");
                    foreach (var photo in photos)
                    {
                        var destinationPath = Path.Combine(Environment.CurrentDirectory, "Photos", photo.Name + ".jpg");
                        hikApi.PhotoService.DownloadFile(session.UserId, photo.Name, photo.Size, destinationPath);
                        Console.WriteLine($"Photo saved to {destinationPath}");
                    }

                    //Get video files for last 4 hours
                    var videos = await hikApi.VideoService.FindFilesAsync(DateTime.Now.AddHours(-4), DateTime.Now, session);
                    Console.WriteLine($"Found {videos.Count} videos");
                    foreach (var video in videos)
                    {
                        var destinationPath = Path.Combine(Environment.CurrentDirectory, "Videos", video.Name + ".mp4");
                        var downloadId = hikApi.VideoService.StartDownloadFile(session.UserId, video.Name, destinationPath);
                        Console.WriteLine($"Downloding {destinationPath}");
                        do
                        {
                            await Task.Delay(5000);
                            int downloadProgress = hikApi.VideoService.GetDownloadPosition(downloadId);
                            Console.WriteLine($"Downloding {downloadProgress} %");
                            if (downloadProgress == 100)
                            {
                                hikApi.VideoService.StopDownloadFile(downloadId);
                                break;
                            }
                            else if (downloadProgress < 0 || downloadProgress > 100)
                            {
                                throw new InvalidOperationException($"UpdateDownloadProgress failed, progress value = {downloadProgress}");
                            }
                        }
                        while (true);
                        Console.WriteLine($"Downloaded {destinationPath}");
                    }
                }

                hikApi.Logout(session.UserId);
                hikApi.Cleanup();
                Console.WriteLine($"Done");
            }
            catch (HikException hikEx)
            {
                Console.WriteLine(hikEx.Message);
                Console.WriteLine(hikEx.ErrorMessage);
                Console.WriteLine(hikEx.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
