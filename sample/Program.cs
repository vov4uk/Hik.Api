using Hik.Api;
using Hik.Api.Abstraction;
using Hik.Api.Data;
using System;
using System.IO;
using System.Linq;

using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class Program
    {
        private static IHikApi hikApi = new HikApi();

        private static Session session;


        static async Task Main(string[] args)
        {
            try
            {

                Console.WriteLine("App started");
                Directory.CreateDirectory("Videos");
                Directory.CreateDirectory("Photos");
 
                hikApi.Initialize();
                hikApi.SetupLogs(3, "C:\\sdkLogsPath", false);
                hikApi.SetConnectTime(2000, 1);
                hikApi.SetReconnect(10000, 1);

                // Please update IP Address, port and user credentials
                session = hikApi.Login("192.168.1.68", 8000, "test", "password_1");

                Console.WriteLine("Login success");

                // For NVR
                if (session.Device.IpChannels.Any())
                {
                    Console.WriteLine($"Found {session.Device.IpChannels} IpChannels");
                    foreach (var channel in session.Device.IpChannels)
                    {
                        Console.WriteLine($"{channel.Name} {channel.ChannelNumber}; IsOnline : {channel.IsOnline}; IsEmpty : {channel.IsEmpty}");
                        if (channel.IsOnline)
                        {
                            var videos = await hikApi.VideoService.FindFilesAsync(DateTime.Now.AddHours(-4), DateTime.Now, session, channel.ChannelNumber);
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
