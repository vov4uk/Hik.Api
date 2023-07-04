using Hik.Api;
using Hik.Api.Abstraction;
using Hik.Api.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class Program
    {
        private static IHikApi hikApi = new HikApi();
        private static int playbackId;
        private static string playbackFileName;
        private static Session session;
        private static List<byte[]> bytes = new List<byte[]>();
        private static int currentFragmentLenght = 0;

        private static int maxFileSizeMb = 10;

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

                // Get Camera time
                var cameraTime = hikApi.GetTime(session.UserId);
                Console.WriteLine($"Camera time :{cameraTime}");

                hikApi.PlaybackService.RealDataReceived += VideoService_RealDataReceived;
                playbackId = hikApi.PlaybackService.StartPlayBack(session.UserId, session.Device.DefaultIpChannel);


                //using (Timer timer = new Timer((o) => DownloadCallback(), null, 0, 15000))
                {
                    Console.WriteLine("Press \'q\' to quit");
                    while (Console.ReadKey() != new ConsoleKeyInfo('q', ConsoleKey.Q, false, false, false))
                    {
                        // do nothing
                    }

                }
                //hikApi.VideoService.StopRecording(playbackId);
                hikApi.PlaybackService.StopPlayBack(playbackId);

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

        private static void VideoService_RealDataReceived(object sender, byte[] e)
        {
            //Console.WriteLine("Received {0} bytes", e.Length);
            currentFragmentLenght += e.Length;
            var filesize = currentFragmentLenght / (1024.0 * 1024.0);
            Console.WriteLine("File size {0} mb", filesize);
            //if (filesize > maxFileSizeMb)
            {
                FileStream fs = null;
                BinaryWriter bw = null;
                try
                {
                    fs = new FileStream($"DecodedVideo.mp4", FileMode.Append);
                    bw = new BinaryWriter(fs);

                    bw.Write(e);

                    bw.Flush();
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    bw.Close();
                    fs.Close();
                }
                currentFragmentLenght= 0;
                bytes.Clear();
            }
        }

        //private static void DownloadCallback()
        //{
        //    DateTime start = DateTime.Now;
        //    var filename = $"{start:yyyyMMdd_HHmmss}.mp4";
        //    if (playbackId == 0)
        //    {
        //        playbackId = hikApi.VideoService.StartPlayBack(session.UserId, session.Device.DefaultIpChannel);
        //        hikApi.VideoService.StartRecording(playbackId, filename, session.UserId, session.Device.DefaultIpChannel);
        //        playbackFileName = filename;
        //    }
        //    else
        //    {
        //        FileInfo file = new FileInfo(playbackFileName);
        //        var filesize = file.Length / (1024.0 * 1024.0);
        //        Console.WriteLine("File size {0} mb", filesize);
        //        if (filesize > maxFileSizeMb)
        //        {
        //            bool playbackStoped = hikApi.VideoService.StopRecording(playbackId);
        //            if (playbackStoped)
        //            {
        //                Console.WriteLine("Stop recording");
        //            }

        //            bool playbackStarted = hikApi.VideoService.StartRecording(playbackId, filename, session.UserId, session.Device.DefaultIpChannel);
        //            playbackFileName = filename;
        //            if (playbackStarted)
        //            {
        //                Console.WriteLine("Start recording");
        //            }

        //            playbackFileName = filename;
        //        }
        //    }
        //}
    }
}
