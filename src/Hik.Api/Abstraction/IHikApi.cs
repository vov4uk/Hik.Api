using Hik.Api.Data;
using Hik.Api.Services;
using System;

namespace Hik.Api.Abstraction
{
    public interface IHikApi
    {
        HikVideoService VideoService { get; }

        HikPhotoService PhotoService { get; }

        PlaybackService PlaybackService { get; }

        bool Initialize();

        bool SetConnectTime(uint waitTimeMilliseconds, uint tryTimes);

        bool SetReconnect(uint interval, int enableRecon);

        bool SetupLogs(int logLevel, string logDirectory, bool autoDelete);

        Session Login(string ipAddress, int port, string userName, string password);

        HdInfo GetHddStatus(int userId);

        DateTime GetTime(int userId);

        void SetTime(DateTime dateTime, int userId);

        DeviceConfig GetDeviceConfig(int userId);

        NetworkConfig GetNetworkConfig(int userId);

        void Logout(int userId);

        void Cleanup();
    }
}
