using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Hik.Api.Abstraction;
using Hik.Api.Data;
using Hik.Api.Helpers;
using Hik.Api.Services;
using Hik.Api.Struct;

namespace Hik.Api
{
    /// <summary>
    /// Implementation of IHikApi
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HikApi : IHikApi, IDisposable
    {
        private static bool initialized = false;
        private IVideoService videoService;
        private IPhotoService pictureService;
        private IPlaybackService playbackService;
        private IConfigService configService;

        internal const string HCNetSDK = @"SDK\HCNetSDK.dll";
        /// <summary>
        /// When connection is lost
        /// </summary>
        public event EventHandler Disconnected;

        private HikApi(int userId, string host, NET_DVR_DEVICEINFO_V30 deviceInfo)
        {
            UserId = userId;
            Host = host;
            DefaultIpChannel = deviceInfo.byChanNum;
            IpChannels = InfoIPChannel(userId, deviceInfo);
        }

        /// <summary>Gets the video service.</summary>
        /// <value>The video service.</value>
        public IVideoService VideoService
        {
            get
            {
                return videoService ??= new VideoService(this);
            }
        }

        /// <summary>Gets the photo service.</summary>
        /// <value>The photo service.</value>
        public IPhotoService PhotoService
        {
            get
            {
                return pictureService ??= new PhotoService(this);
            }
        }

        /// <summary>Gets the playback service.</summary>
        /// <value>The playback service.</value>
        public IPlaybackService PlaybackService
        {
            get
            {
                return playbackService ??= new PlaybackService(this);
            }
        }

        /// <summary>
        /// Config service
        /// </summary>
        public IConfigService ConfigService
        {
            get
            {
                return configService ??= new ConfigService(this);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="HikApi" /> is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if connected; otherwise, <c>false</c>.
        /// </value>
        public bool Connected { get; private set; } = true;

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public int UserId { get; }

        /// <summary>
        /// Gets the Host identifier.
        /// </summary>
        /// <value>
        /// The Host identifier.
        /// </value>
        public string Host { get; }

        /// <summary>Gets the default ip channel.</summary>
        /// <value>The default ip channel.</value>
        public byte DefaultIpChannel { get; }

        /// <summary>Gets the ip channels.</summary>
        /// <value>The ip channels.</value>
        public IReadOnlyCollection<IpChannel> IpChannels { get; }

        /// <summary>
        /// Initialize the SDK and call other SDK functions.
        /// </summary>
        /// <param name="logLevel">[in] Log level. 0- close log(default), 1- output ERROR log only, 2- output ERROR and DEBUG log, 3- output all log, including ERROR, DEBUG and INFO log</param>
        /// <param name="logDirectory">[in] Log file saving path, if set to NULL, the default path for Windows is "C:\\SdkLog\\", and the default path for Linux is ""/home/sdklog/" </param>
        /// <param name="autoDeleteLogs">[bool] Whether to delete the files which exceed the number limit. Default: TRUE</param>
        /// <param name="waitTimeMilliseconds">Timeout,unit: ms, value range: [300,75000], the actual max timeout time is different with different system connecting timeout</param>
        /// <param name="tryTimes">Connecting attempt times (reserved)</param>
        /// <param name="reconnectInterval">Reconnecting interval, unit: milliseconds, default value:30 seconds</param>
        /// <param name="enableReconnect">Enable or disable reconnect function, 0-disable, 1-enable(default)</param>
        /// <returns>TRUE means success, FALSE means failure. </returns>
        /// <remarks>This API is used to initialize SDK. Please call this API before calling any other API</remarks>
        public static void Initialize(int logLevel = 3, string logDirectory = "HikvisionSDKLogs", bool autoDeleteLogs = true, uint waitTimeMilliseconds = 2000, uint tryTimes = 1, uint reconnectInterval = 10000, bool enableReconnect = true)
        {
            if (initialized == false)
            {
                SdkHelper.InvokeSDK(() => NET_DVR_Init());
                SdkHelper.InvokeSDK(() => NET_DVR_SetLogToFile(logLevel, logDirectory, autoDeleteLogs));
                SdkHelper.InvokeSDK(() => NET_DVR_SetConnectTime(waitTimeMilliseconds, tryTimes));
                SdkHelper.InvokeSDK(() => NET_DVR_SetReconnect(reconnectInterval, enableReconnect ? 1 : 0));
                initialized = true;
            }
        }

        /// <summary>
        /// This API is used to login user to the device.
        /// </summary>
        /// <param name="ipAddress">device IP address</param>
        /// <param name="port">device port number</param>
        /// <param name="userName">Login username</param>
        /// <param name="password">password.</param>
        /// <returns>User session</returns>
        /// <remarks>It supports 32 different user names for DS7116, DS81xx, DS90xx and DS91xx series devices, and 128 users login at the same time.Other devices support 16 different user names and 128 users login at the same time. SDK supports 512 * login.UserID is incremented one by one, from 0 to 511 and then return to 0. Logout and NET_DVR_Cleanup will not initialize the UserID to 0. If client offline abnormally, the device will keep the UserID 5 minutes, and the UserID will invalid after the valid time.</remarks>
        public static IHikApi Login(string ipAddress, int port, string userName, string password)
        {
            NET_DVR_DEVICEINFO_V30 deviceInfo = new NET_DVR_DEVICEINFO_V30();
            int userId = SdkHelper.InvokeSDK(() => NET_DVR_Login_V30(ipAddress, port, userName, password, ref deviceInfo));

            return new HikApi(userId, ipAddress, deviceInfo);
        }

        /// <summary>
        /// Release SDK resources, last call before the end
        /// </summary>
        /// <returns>TRUE means success, FALSE means failure</returns>
        /// <remarks>This API is used to release SDK resource. Please calling it before closing the program.</remarks>
        public static void Cleanup() => SdkHelper.InvokeSDK(() => NET_DVR_Cleanup());

        /// <summary>
        /// This API is used to logout certain user.
        /// </summary>
        /// <returns>
        /// TRUE means success, FALSE means failure
        /// </returns>
        /// <remarks>
        /// It is suggested to call this API to logout.
        /// </remarks>
        public void Logout()
        {
            if (!Connected)
                return;
            SdkHelper.InvokeSDK(() => NET_DVR_Logout(UserId), throwException: false);
            OnDisconnected();
        }

        /// <summary>
        ///  Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Logout();
        }

        private void OnDisconnected()
        {
            Connected = false;
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private List<IpChannel> InfoIPChannel(int userId, NET_DVR_DEVICEINFO_V30 deviceInfo)
        {
            var ipChannels = new List<IpChannel>();
            uint dwAChanTotalNum = deviceInfo.byChanNum;
            uint dwDChanTotalNum = deviceInfo.byIPChanNum + 256 * (uint)deviceInfo.byHighDChanNum;
            if (dwDChanTotalNum > 0)
            {
                NET_DVR_IPPARACFG_V40 ipParaCfgV40 = default;

                int dwSize = Marshal.SizeOf(ipParaCfgV40);

                IntPtr ptrIpParaCfgV40 = Marshal.AllocHGlobal(dwSize);
                Marshal.StructureToPtr(ipParaCfgV40, ptrIpParaCfgV40, false);

                uint dwReturn = 0;
                int iGroupNo = 0;

                var ipChannelsConfig = SdkHelper.InvokeSDK(() => Services.ConfigService.NET_DVR_GetDVRConfig(userId, HikConst.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, (uint)dwSize, ref dwReturn));
                if (ipChannelsConfig)
                {
                    // success
                    ipParaCfgV40 = (NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIpParaCfgV40, typeof(NET_DVR_IPPARACFG_V40));

                    for (int i = 0; i < dwAChanTotalNum; i++)
                    {
                        var channelNumber = i + (int)deviceInfo.byStartChan;
                        var channel = ListAnalogChannel(channelNumber, ipParaCfgV40.byAnalogChanEnable[i]);
                        ipChannels.Add(channel);
                    }

                    byte byStreamType;
                    uint iDChanNum = 64;

                    if (dwDChanTotalNum < 64)
                    {
                        //If the ip channels of device is less than 64,will get the real channel of device
                        iDChanNum = dwDChanTotalNum;
                    }

                    for (int i = 0; i < iDChanNum; i++)
                    {
                        byStreamType = ipParaCfgV40.struStreamMode[i].byGetStreamType;

                        dwSize = Marshal.SizeOf(ipParaCfgV40.struStreamMode[i].uGetStream);
                        switch (byStreamType)
                        {
                            //NVR supports only the mode: get stream from device directly
                            case 0:
                                IntPtr ptrChanInfo = Marshal.AllocHGlobal(dwSize);
                                Marshal.StructureToPtr(ipParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfo, false);
                                NET_DVR_IPCHANINFO channelInfo = (NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(NET_DVR_IPCHANINFO));

                                //List the IP channel
                                var ipChannelNumber = channelInfo.byIPID + channelInfo.byIPIDHigh * 256 - iGroupNo * 64 - 1;
                                ipChannels.Add(ListIPChannel(ipChannelNumber, channelInfo.byEnable, channelInfo.byIPID));

                                Marshal.FreeHGlobal(ptrChanInfo);
                                break;
                            case 6:
                                IntPtr ptrChanInfoV40 = Marshal.AllocHGlobal(dwSize);
                                Marshal.StructureToPtr(ipParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfoV40, false);
                                NET_DVR_IPCHANINFO_V40 channelInfoV40 = (NET_DVR_IPCHANINFO_V40)Marshal.PtrToStructure(ptrChanInfoV40, typeof(NET_DVR_IPCHANINFO_V40));

                                //List the IP channel
                                var ipChannelNumberV40 = channelInfoV40.wIPID - iGroupNo * 64 - 1;
                                ipChannels.Add(ListIPChannel(ipChannelNumberV40, channelInfoV40.byEnable, channelInfoV40.wIPID));

                                Marshal.FreeHGlobal(ptrChanInfoV40);
                                break;
                            default:
                                break;
                        }
                    }
                }
                Marshal.FreeHGlobal(ptrIpParaCfgV40);
            }
            else
            {
                for (int i = 0; i < dwAChanTotalNum; i++)
                {
                    var channelNumber = i + deviceInfo.byStartChan;
                    ipChannels.Add(ListAnalogChannel(channelNumber, 1));
                }
            }

            return ipChannels;
        }

        private IpChannel ListIPChannel(int iChanNo, byte byOnline, int byIPID)
        {
            string str2;

            if (byIPID == 0)
            {
                str2 = "X"; //the channel is idle
            }
            else
            {
                if (byOnline == 0)
                {
                    str2 = "offline"; //the channel is off-line
                }
                else
                    str2 = "online"; //The channel is on-line
            }

            var str1 = String.Format("IPCamera {0} {1}", iChanNo, str2);
            return new IpChannel(iChanNo, byOnline != 0, str1);
        }
        private IpChannel ListAnalogChannel(int iChanNo, byte byEnable)
        {
            string str2;
            if (byEnable == 0)
            {
                str2 = "Disabled"; //This channel has been disabled
            }
            else
            {
                str2 = "Enabled"; //This channel has been enabled
            }
            var str1 = string.Format("Camera {0} {1}", iChanNo, str2);
            return new IpChannel(iChanNo, byEnable != 0, str1);
        }

        /// <summary>
        /// Initialize the SDK and call other SDK functions.
        /// </summary>
        /// <returns>TRUE means success, FALSE means failure. </returns>
        /// <remarks>This API is used to initialize SDK. Please call this API before calling any other API</remarks>
        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_Init();

        /// <summary>
        /// Enable log file writing interface
        /// </summary>
        /// <param name="bLogEnable">The level of the log (default is 0): 0- means to close the log, 1- means only output ERROR error log, 2- output ERROR error information and DEBUG debugging information, 3- output ERROR All information such as error information, DEBUG debugging information and INFO general information</param>
        /// <param name="strLogDir">The path of the log file, the default value of windows is "C:\\SdkLog\\"; the default value of linux is "/home/sdklog/"</param>
        /// <param name="bAutoDel">Whether to delete the excess number of files, the default value is TRUE.. When it is TRUE, it means the overwrite mode. When the number of log files exceeds the SDK limit, the excess files will be automatically deleted. The SDK limit is 10 by default</param>
        /// <returns>Return TRUE on success, FALSE on failure. Please call NET_DVR_GetLastError to get the error code.</returns>
        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_SetLogToFile(int bLogEnable, string strLogDir, bool bAutoDel);

        /// <summary>
        /// This API is used to login user to the device.
        /// </summary>
        /// <param name="sDVRIP">device IP address</param>
        /// <param name="wDVRPort">device port number</param>
        /// <param name="sUserName">Login username</param>
        /// <param name="sPassword">password.</param>
        /// <param name="lpDeviceInfo">device information.</param>
        /// <returns>-1 indicates failure, other values indicate the returned user ID value</returns>
        /// <remarks>It supports 32 different user names for DS7116, DS81xx, DS90xx and DS91xx series devices, and 128 users login at the same time.Other devices support 16 different user names and 128 users login at the same time. SDK supports 512 * login.UserID is incremented one by one, from 0 to 511 and then return to 0. Logout and NET_DVR_Cleanup will not initialize the UserID to 0. If client offline abnormally, the device will keep the UserID 5 minutes, and the UserID will invalid after the valid time.</remarks>
        [DllImport(HCNetSDK)]
        private static extern int NET_DVR_Login_V30(string sDVRIP, int wDVRPort, string sUserName, string sPassword, ref NET_DVR_DEVICEINFO_V30 lpDeviceInfo);

        /// <summary>
        /// This API is used to logout certain user.
        /// </summary>
        /// <param name="iUserID">User ID, the return value of NET_DVR_Login_V30</param>
        /// <returns>TRUE means success, FALSE means failure</returns>
        /// <remarks>It is suggested to call this API to logout.</remarks>
        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_Logout(int iUserID);

        /// <summary>
        /// Release SDK resources, last call before the end
        /// </summary>
        /// <returns>TRUE means success, FALSE means failure</returns>
        /// <remarks>This API is used to release SDK resource. Please calling it before closing the program.</remarks>
        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_Cleanup();

        /// <summary>
        /// Set the network connection timeout and the number of connection attempts
        /// </summary>
        /// <param name="dwWaitTime">Timeout,unit: ms, value range: [300,75000], the actual max timeout time is different with different system connecting timeout</param>
        /// <param name="dwTryTimes">Connecting attempt times (reserved)</param>
        /// <returns>Return TRUE on success, FALSE on failure. Please call NET_DVR_GetLastError to get the error code.</returns>
        /// <remarks>Default timeout of SDK to establish a connection is 3 seconds. Interface will not return FASLE when the set timeout value is greater or less than the limit, it will take the nearest upper and lower limit value as the actual timeout.</remarks>
        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_SetConnectTime(uint dwWaitTime, uint dwTryTimes);

        /// <summary>
        /// Set the reconnection function.
        /// </summary>
        /// <param name="dwInterval">Reconnecting interval, unit: milliseconds, default value:30 seconds</param>
        /// <param name="bEnableRecon">Enable or disable reconnect function, 0-disable, 1-enable(default)</param>
        /// <returns>Return TRUE on success, FALSE on failure. Please call NET_DVR_GetLastError to get the error code.</returns>
        /// <remarks>This API can set the reconnect function for preview, transparent channel and alar on guard state.If the user does not call this API, the SDK will initial the reconnect function for preview, transparent channel and alarm on guard state by default, and the reconnect interval is 5 seconds.</remarks>
        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_SetReconnect(uint dwInterval, int bEnableRecon);
    }
}