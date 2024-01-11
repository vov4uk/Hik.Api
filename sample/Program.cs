using Hik.Api;
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

                HikApi.Initialize();

                // Please update IP Address, port and user credentials
                var hikApi = HikApi.Login("192.168.1.64", 8000, "admin", "password");
                Console.WriteLine("Login success");

                // Get Camera time
                var cameraTime = hikApi.ConfigService.GetTime();
                Console.WriteLine($"Camera time :{cameraTime}");
                var currentTime = DateTime.Now;
                if (Math.Abs((currentTime - cameraTime).TotalSeconds) > 5)
                {
                    hikApi.ConfigService.SetTime(currentTime);
                }

                // GetNetworkConfig
                var network = hikApi.ConfigService.GetNetworkConfig();
                Console.WriteLine(JsonConvert.SerializeObject(network, Formatting.Indented));

                // GetDeviceConfig
                var device = hikApi.ConfigService.GetDeviceConfig();
                Console.WriteLine(JsonConvert.SerializeObject(device, Formatting.Indented));

                // For NVR
                if (hikApi.IpChannels.Any())
                {
                    Console.WriteLine($"Found {hikApi.IpChannels.Count} IpChannels");
                    foreach (var channel in hikApi.IpChannels)
                    {
                        Console.WriteLine($"IP Channel {channel.ChannelNumber}; IsOnline : {channel.IsOnline};");
                        if (channel.IsOnline)
                        {
                            var videos = await hikApi.VideoService.FindFilesAsync(DateTime.Now.AddHours(-4), DateTime.Now, channel.ChannelNumber);
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
                    var photos = await hikApi.PhotoService.FindFilesAsync(DateTime.Now.AddHours(-2), DateTime.Now);
                    Console.WriteLine($"Found {photos.Count} photos");
                    foreach (var photo in photos)
                    {
                        var destinationPath = Path.Combine(Environment.CurrentDirectory, "Photos", photo.Name + ".jpg");
                        hikApi.PhotoService.DownloadFile(photo.Name, photo.Size, destinationPath);
                        Console.WriteLine($"Photo saved to {destinationPath}");
                    }

                    //Get video files for last 4 hours
                    var videos = await hikApi.VideoService.FindFilesAsync(DateTime.Now.AddHours(-4), DateTime.Now);
                    Console.WriteLine($"Found {videos.Count} videos");
                    foreach (var video in videos)
                    {
                        var destinationPath = Path.Combine(Environment.CurrentDirectory, "Videos", video.Name + ".mp4");
                        var downloadId = hikApi.VideoService.StartDownloadFile(video.Name, destinationPath);
                        Console.WriteLine($"Downloading {destinationPath}");
                        do
                        {
                            await Task.Delay(5000);
                            int downloadProgress = hikApi.VideoService.GetDownloadPosition(downloadId);
                            Console.WriteLine($"Downloading {downloadProgress} %");
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

                hikApi.Logout();
                HikApi.Cleanup();
                Console.WriteLine($"Done");
            }
            catch (HikException hikEx)
            {
                Console.WriteLine(hikEx.ToString());
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
