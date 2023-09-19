using Hik.Api.Data;
using Hik.Api.Services;
using System;

namespace Hik.Api.Abstraction
{
    /// <summary>
    /// Hikvision SDK Wrapper
    /// </summary>
    public interface IHikApi
    {

        /// <summary>
        /// Gets the video service.
        /// </summary>
        /// <value>
        /// The video service.
        /// </value>
        HikVideoService VideoService { get; }


        /// <summary>
        /// Gets the photo service.
        /// </summary>
        /// <value>
        /// The photo service.
        /// </value>
        HikPhotoService PhotoService { get; }


        /// <summary>
        /// Gets the playback service.
        /// </summary>
        /// <value>
        /// The playback service.
        /// </value>
        PlaybackService PlaybackService { get; }

        /// <summary>Initializes this instance.</summary>
        /// <returns>
        ///   <br />
        /// </returns>
        bool Initialize();

        /// <summary>Sets the connect time.</summary>
        /// <param name="waitTimeMilliseconds">The wait time milliseconds.</param>
        /// <param name="tryTimes">The try times.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        bool SetConnectTime(uint waitTimeMilliseconds, uint tryTimes);

        /// <summary>
        /// Sets the reconnect.
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <param name="enableRecon">The enable recon.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        bool SetReconnect(uint interval, int enableRecon);

        /// <summary>
        /// Setups the logs.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="logDirectory">The log directory.</param>
        /// <param name="autoDelete">if set to <c>true</c> [automatic delete].</param>
        /// <returns>
        ///   <br />
        /// </returns>
        bool SetupLogs(int logLevel, string logDirectory, bool autoDelete);

        /// <summary>
        /// Logins the specified ip address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        Session Login(string ipAddress, int port, string userName, string password);

        /// <summary>
        /// Get SD Card info, capaity, free space, status etc.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="ipChannel">The ip channel.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        HdInfo GetHddStatus(int userId, int ipChannel = -1);

        /// <summary>
        /// Gets the time.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="ipChannel">The ip channel.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        DateTime GetTime(int userId, int ipChannel = -1);

        /// <summary>
        /// Sets the time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="ipChannel">The ip channel.</param>
        void SetTime(DateTime dateTime, int userId, int ipChannel = -1);

        /// <summary>Gets the device configuration.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        DeviceConfig GetDeviceConfig(int userId);

        /// <summary>Gets the network configuration.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        NetworkConfig GetNetworkConfig(int userId);

        /// <summary>Logouts the specified user identifier.</summary>
        /// <param name="userId">The user identifier.</param>
        void Logout(int userId);

        /// <summary>Cleanups this instance.</summary>
        void Cleanup();
    }
}
