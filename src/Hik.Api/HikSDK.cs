using Hik.Api.Abstraction;

namespace Hik.Api
{
    /// <summary>
    /// HikSDK
    /// </summary>
    /// <seealso cref="Hik.Api.Abstraction.IHikSDK" />
    public class HikSDK : IHikSDK
    {
        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        public void Cleanup()
        {
            HikApi.Cleanup();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="logDirectory">The log directory.</param>
        /// <param name="autoDeleteLogs">if set to <c>true</c> [automatic delete logs].</param>
        /// <param name="waitTimeMilliseconds">The wait time milliseconds.</param>
        /// <param name="tryTimes">The try times.</param>
        /// <param name="reconnectInterval">The reconnect interval.</param>
        /// <param name="enableReconnect">if set to <c>true</c> [enable reconnect].</param>
        public void Initialize(int logLevel = 3, string logDirectory = "HikvisionSDKLogs", bool autoDeleteLogs = true, uint waitTimeMilliseconds = 2000, uint tryTimes = 1, uint reconnectInterval = 10000, bool enableReconnect = true)
        {
            HikApi.Initialize();
        }

        /// <summary>
        /// Logins the specified ip address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public IHikApi Login(string ipAddress, int port, string userName, string password)
        {
            return HikApi.Login(ipAddress, port, userName, password);
        }
    }
}
