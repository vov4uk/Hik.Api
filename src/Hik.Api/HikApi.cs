using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Hik.Api.Abstraction;
using Hik.Api.Data;
using Hik.Api.Helpers;
using Hik.Api.Services;
using Hik.Api.Struct;
using Hik.Api.Struct.Config;

namespace Hik.Api
{
    /// <summary>
    /// Implementation of IHikApi
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class HikApi : IHikApi
    {
        private HikVideoService videoService;
        private HikPhotoService pictureService;
        private PlaybackService playbackService;

        private uint dwAChanTotalNum = 0;
        private uint dwDChanTotalNum = 0;

        private NET_DVR_IPCHANINFO struChanInfo;
        private NET_DVR_IPCHANINFO_V40 struChanInfoV40;

        internal const string HCNetSDK = @"SDK\HCNetSDK.dll";

        /// <summary>Gets the video service.</summary>
        /// <value>The video service.</value>
        public HikVideoService VideoService
        {
            get
            {
                return videoService ??= new HikVideoService();
            }
        }

        /// <summary>Gets the photo service.</summary>
        /// <value>The photo service.</value>
        public HikPhotoService PhotoService
        {
            get
            {
                return pictureService ??= new HikPhotoService();
            }
        }

        /// <summary>Gets the playback service.</summary>
        /// <value>The playback service.</value>
        public PlaybackService PlaybackService
        {
            get
            {
                return playbackService ??= new PlaybackService();
            }
        }

        /// <summary>
        /// Initialize the SDK and call other SDK functions.
        /// </summary>
        /// <returns>TRUE means success, FALSE means failure. </returns>
        /// <remarks>This API is used to initialize SDK. Please call this API before calling any other API</remarks>
        public bool Initialize() => SdkHelper.InvokeSDK(() => NET_DVR_Init());

        /// <summary>
        /// Set the network connection timeout and the number of connection attempts
        /// </summary>
        /// <param name="waitTimeMilliseconds">Timeout,unit: ms, value range: [300,75000], the actual max timeout time is different with different system connecting timeout</param>
        /// <param name="tryTimes">Connecting attempt times (reserved)</param>
        /// <returns>Return TRUE on success, FALSE on failure.</returns>
        /// <remarks>Default timeout of SDK to establish a connection is 3 seconds. Interface will not return FASLE when the set timeout value is greater or less than the limit, it will take the nearest upper and lower limit value as the actual timeout.</remarks>
        public bool SetConnectTime(uint waitTimeMilliseconds, uint tryTimes)
            => SdkHelper.InvokeSDK(() => NET_DVR_SetConnectTime(waitTimeMilliseconds, tryTimes)); // 2000 , 1

        /// <summary>
        /// Set the reconnection function.
        /// </summary>
        /// <param name="interval">Reconnecting interval, unit: milliseconds, default value:30 seconds</param>
        /// <param name="enableRecon">Enable or disable reconnect function, 0-disable, 1-enable(default)</param>
        /// <returns>Return TRUE on success, FALSE on failure.</returns>
        /// <remarks>This API can set the reconnect function for preview, transparent channel and alar on guard state.If the user does not call this API, the SDK will initial the reconnect function for preview, transparent channel and alarm on guard state by default, and the reconnect interval is 5 seconds.</remarks>
        public bool SetReconnect(uint interval, int enableRecon)
            => SdkHelper.InvokeSDK(() => NET_DVR_SetReconnect(interval, enableRecon)); // 10000 , 1

        /// <summary>
        /// This API is used to start writing log file.
        /// </summary>
        /// <param name="logLevel">[in] Log level. 0- close log(default), 1- output ERROR log only, 2- output ERROR and DEBUG log, 3- output all log, including ERROR, DEBUG and INFO log</param>
        /// <param name="logDirectory">[in] Log file saving path, if set to NULL, the default path for Windows is "C:\\SdkLog\\", and the default path for Linux is ""/home/sdklog/" </param>
        /// <param name="autoDelete">[bool] Whether to delete the files which exceed the number limit. Default: TRUE</param>
        /// <returns>
        /// Return TRUE on success, FALSE on failure.
        /// </returns>
        public bool SetupLogs(int logLevel, string logDirectory, bool autoDelete) => SdkHelper.InvokeSDK(() => NET_DVR_SetLogToFile(logLevel, logDirectory, autoDelete));

        /// <summary>
        /// This API is used to login user to the device.
        /// </summary>
        /// <param name="ipAddress">device IP address</param>
        /// <param name="port">device port number</param>
        /// <param name="userName">Login username</param>
        /// <param name="password">password.</param>
        /// <returns>User session</returns>
        /// <remarks>It supports 32 different user names for DS7116, DS81xx, DS90xx and DS91xx series devices, and 128 users login at the same time.Other devices support 16 different user names and 128 users login at the same time. SDK supports 512 * login.UserID is incremented one by one, from 0 to 511 and then return to 0. Logout and NET_DVR_Cleanup will not initialize the UserID to 0. If client offline abnormally, the device will keep the UserID 5 minutes, and the UserID will invalid after the valid time.</remarks>
        public Session Login(string ipAddress, int port, string userName, string password)
        {
            NET_DVR_DEVICEINFO_V30 deviceInfo = new NET_DVR_DEVICEINFO_V30();
            int userId = SdkHelper.InvokeSDK(() => NET_DVR_Login_V30(ipAddress, port, userName, password, ref deviceInfo));

            var channels = InfoIPChannel(userId, deviceInfo);
            return new Session(userId, deviceInfo.byChanNum, channels);
        }

        /// <summary>
        /// Release SDK resources, last call before the end
        /// </summary>
        /// <returns>TRUE means success, FALSE means failure</returns>
        /// <remarks>This API is used to release SDK resource. Please calling it before closing the program.</remarks>
        public void Cleanup() => SdkHelper.InvokeSDK(() => NET_DVR_Cleanup());

        /// <summary>
        /// This API is used to logout certain user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        /// TRUE means success, FALSE means failure
        /// </returns>
        /// <remarks>
        /// It is suggested to call this API to logout.
        /// </remarks>
        public void Logout(int userId) => SdkHelper.InvokeSDK(() => NET_DVR_Logout(userId));

        /// <summary>
        /// Get SD Card info, capaity, free space, status etc.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="ipChannel">Default value -1</param>
        /// <returns>HdInfo</returns>
        public HdInfo GetHddStatus(int userId, int ipChannel = -1)
        {
            NET_DVR_HDCFG hdConfig = default;
            uint returned = 0;
            int sizeOfConfig = Marshal.SizeOf(hdConfig);
            IntPtr ptrDeviceCfg = Marshal.AllocHGlobal(sizeOfConfig);
            Marshal.StructureToPtr(hdConfig, ptrDeviceCfg, false);
            SdkHelper.InvokeSDK(() => NET_DVR_GetDVRConfig(
                userId,
                HikConst.NET_DVR_GET_HDCFG,
                ipChannel,
                ptrDeviceCfg,
                (uint)sizeOfConfig,
                ref returned));

            hdConfig = (NET_DVR_HDCFG)Marshal.PtrToStructure(ptrDeviceCfg, typeof(NET_DVR_HDCFG));
            Marshal.FreeHGlobal(ptrDeviceCfg);
            return new HdInfo(hdConfig.struHDInfo[0]);
        }

        /// <summary>
        /// Get device current time
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="ipChannel">Default value 1</param>
        /// <returns></returns>
        public DateTime GetTime(int userId, int ipChannel = 1)
        {
            NET_DVR_TIME m_struTimeCfg = default;

            uint dwReturn = 0;
            int nSize = Marshal.SizeOf(m_struTimeCfg);
            IntPtr ptrTimeCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struTimeCfg, ptrTimeCfg, false);

            SdkHelper.InvokeSDK(() => NET_DVR_GetDVRConfig(userId, HikConst.NET_DVR_GET_TIMECFG, ipChannel, ptrTimeCfg, (uint)nSize, ref dwReturn));

            m_struTimeCfg = (NET_DVR_TIME)Marshal.PtrToStructure(ptrTimeCfg, typeof(NET_DVR_TIME));

            Marshal.FreeHGlobal(ptrTimeCfg);

            return m_struTimeCfg.ToDateTime();
        }

        /// <summary>
        /// Set device current time
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="ipChannel">Default value -1</param>
        public void SetTime(DateTime dateTime, int userId, int ipChannel = -1)
        {
            NET_DVR_TIME m_struTimeCfg = new NET_DVR_TIME(dateTime);
            int nSize = Marshal.SizeOf(m_struTimeCfg);
            IntPtr ptrTimeCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struTimeCfg, ptrTimeCfg, false);

            SdkHelper.InvokeSDK(() => NET_DVR_SetDVRConfig(userId, HikConst.NET_DVR_SET_TIMECFG, ipChannel, ptrTimeCfg, (uint)nSize));
            Marshal.FreeHGlobal(ptrTimeCfg);
        }

        /// <summary>
        /// Get device configuration information.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DeviceConfig GetDeviceConfig(int userId)
        {
            NET_DVR_DEVICECFG_V40 m_struDeviceCfg = default;

            uint dwReturn = 0;
            int nSize = Marshal.SizeOf(m_struDeviceCfg);
            IntPtr ptrDeviceCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struDeviceCfg, ptrDeviceCfg, false);

            SdkHelper.InvokeSDK(() => NET_DVR_GetDVRConfig(userId, HikConst.NET_DVR_GET_DEVICECFG_V40, -1, ptrDeviceCfg, (uint)nSize, ref dwReturn));

            m_struDeviceCfg = (NET_DVR_DEVICECFG_V40)Marshal.PtrToStructure(ptrDeviceCfg, typeof(NET_DVR_DEVICECFG_V40));

            uint iVer1 = (m_struDeviceCfg.dwSoftwareVersion >> 24) & 0xFF;
            uint iVer2 = (m_struDeviceCfg.dwSoftwareVersion >> 16) & 0xFF;
            uint iVer3 = m_struDeviceCfg.dwSoftwareVersion & 0xFFFF;
            uint iVer4 = (m_struDeviceCfg.dwSoftwareBuildDate >> 16) & 0xFFFF;
            uint iVer5 = (m_struDeviceCfg.dwSoftwareBuildDate >> 8) & 0xFF;
            uint iVer6 = m_struDeviceCfg.dwSoftwareBuildDate & 0xFF;

            var deviceConfig = new DeviceConfig
            {
                Name = System.Text.Encoding.UTF8.GetString(m_struDeviceCfg.sDVRName).Trim('\0'),
                TypeName = System.Text.Encoding.UTF8.GetString(m_struDeviceCfg.byDevTypeName).Trim('\0'),
                AnalogChannel = Convert.ToInt32(m_struDeviceCfg.byChanNum),
                IPChannel = Convert.ToInt32(m_struDeviceCfg.byIPChanNum + 256 * m_struDeviceCfg.byHighIPChanNum),
                ZeroChannel = Convert.ToInt32(m_struDeviceCfg.byZeroChanNum),
                NetworkPort = Convert.ToInt32(m_struDeviceCfg.byNetworkPortNum),
                AlarmInPort = Convert.ToInt32(m_struDeviceCfg.byAlarmInPortNum),
                AlarmOutPort = Convert.ToInt32(m_struDeviceCfg.byAlarmOutPortNum),
                Serial = System.Text.Encoding.UTF8.GetString(m_struDeviceCfg.sSerialNumber).Trim('\0'),
                Version = $"V{iVer1}.{iVer2}.{iVer3} Build {iVer4,0:D2}{iVer5,0:D2}{iVer6,0:D2}"
            };

            Marshal.FreeHGlobal(ptrDeviceCfg);
            return deviceConfig;
        }

        /// <summary>
        /// Get device network information.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>
        /// Network information
        /// </returns>
        public NetworkConfig GetNetworkConfig(int userId)
        {
            NET_DVR_NETCFG_V30 m_struNetCfg = default;
            uint dwReturn = 0;
            int nSize = Marshal.SizeOf(m_struNetCfg);
            IntPtr ptrNetCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struNetCfg, ptrNetCfg, false);

            SdkHelper.InvokeSDK(() => NET_DVR_GetDVRConfig(userId, HikConst.NET_DVR_GET_NETCFG_V30, -1, ptrNetCfg, (uint)nSize, ref dwReturn));
            m_struNetCfg = (NET_DVR_NETCFG_V30)Marshal.PtrToStructure(ptrNetCfg, typeof(NET_DVR_NETCFG_V30));

            NetworkConfig networkConfig = new NetworkConfig
            {
                IPAddress = m_struNetCfg.struEtherNet[0].struDVRIP.sIpV4,
                GateWay = m_struNetCfg.struGatewayIpAddr.sIpV4,
                SubMask = m_struNetCfg.struEtherNet[0].struDVRIPMask.sIpV4,
                Dns = m_struNetCfg.struDnsServer1IpAddr.sIpV4,
                HostIP = m_struNetCfg.struAlarmHostIpAddr.sIpV4,
                AlarmHostIpPort = Convert.ToInt32(m_struNetCfg.wAlarmHostIpPort),
                HttpPort = Convert.ToInt32(m_struNetCfg.wHttpPortNo),
                DVRPort = Convert.ToInt32(m_struNetCfg.struEtherNet[0].wDVRPort),
                DHCP = m_struNetCfg.byUseDhcp == 1,
                PPPoE = m_struNetCfg.struPPPoE.dwPPPOE == 1,
                PPPoEName = System.Text.Encoding.UTF8.GetString(m_struNetCfg.struPPPoE.sPPPoEUser).Trim('\0'),
                PPPoEPassword = m_struNetCfg.struPPPoE.sPPPoEPassword
            };

            Marshal.FreeHGlobal(ptrNetCfg);
            return networkConfig;
        }

        private List<IpChannel> InfoIPChannel(int userId, NET_DVR_DEVICEINFO_V30 deviceInfo)
        {
            var ipChannels = new List<IpChannel>();

            dwAChanTotalNum = deviceInfo.byChanNum;
            dwDChanTotalNum = deviceInfo.byIPChanNum + 256 * (uint)deviceInfo.byHighDChanNum;
            if (dwDChanTotalNum > 0)
            {
                NET_DVR_IPPARACFG_V40 struIpParaCfgV40 = default;

                int dwSize = Marshal.SizeOf(struIpParaCfgV40);

                IntPtr ptrIpParaCfgV40 = Marshal.AllocHGlobal(dwSize);
                Marshal.StructureToPtr(struIpParaCfgV40, ptrIpParaCfgV40, false);

                uint dwReturn = 0;
                int iGroupNo = 0;

                var ipChannelsConfig = SdkHelper.InvokeSDK(() => NET_DVR_GetDVRConfig(userId, HikConst.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, (uint)dwSize, ref dwReturn));
                if (ipChannelsConfig)
                {
                    // success
                    struIpParaCfgV40 = (NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIpParaCfgV40, typeof(NET_DVR_IPPARACFG_V40));

                    for (int i = 0; i < dwAChanTotalNum; i++)
                    {
                        var channelNumber = i + (int)deviceInfo.byStartChan;
                        var channel = ListAnalogChannel(channelNumber, struIpParaCfgV40.byAnalogChanEnable[i]);
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
                        byStreamType = struIpParaCfgV40.struStreamMode[i].byGetStreamType;

                        dwSize = Marshal.SizeOf(struIpParaCfgV40.struStreamMode[i].uGetStream);
                        switch (byStreamType)
                        {
                            //NVR supports only the mode: get stream from device directly
                            case 0:
                                IntPtr ptrChanInfo = Marshal.AllocHGlobal(dwSize);
                                Marshal.StructureToPtr(struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfo, false);
                                struChanInfo = (NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(NET_DVR_IPCHANINFO));

                                //List the IP channel
                                var ipChannelNumber = struChanInfo.byIPID + struChanInfo.byIPIDHigh * 256 - iGroupNo * 64 - 1;
                                ipChannels.Add(ListIPChannel(ipChannelNumber, struChanInfo.byEnable, struChanInfo.byIPID));

                                Marshal.FreeHGlobal(ptrChanInfo);
                                break;
                            case 6:
                                IntPtr ptrChanInfoV40 = Marshal.AllocHGlobal(dwSize);
                                Marshal.StructureToPtr(struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfoV40, false);
                                struChanInfoV40 = (NET_DVR_IPCHANINFO_V40)Marshal.PtrToStructure(ptrChanInfoV40, typeof(NET_DVR_IPCHANINFO_V40));

                                //List the IP channel
                                var ipChannelNumberV40 = struChanInfoV40.wIPID - iGroupNo * 64 - 1;
                                ipChannels.Add(ListIPChannel(ipChannelNumberV40, struChanInfoV40.byEnable, struChanInfoV40.wIPID));

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

        /// <summary>
        /// Get device configuration information.
        /// </summary>
        /// <param name="lUserID">Return value of login interface such as NET_DVR_Login_V40</param>
        /// <param name="dwCommand">[in] Channel number, different commands correspond to different values, if this parameter is invalid, set it to 0xFFFFFFFF, see "Remarks" for details.</param>
        /// <param name="lChannel"> Channel number, different commands correspond to different values, if this parameter is invalid, set it to 0xFFFFFFFF, see "Remarks" for details.</param>
        /// <param name="lpOutBuffer">[out] Buffer pointer for receiving data</param>
        /// <param name="dwOutBufferSize">[in] The buffer length of received data (in bytes), which cannot be 0</param>
        /// <param name="lpBytesReturned">[out] The actual received data length pointer, which cannot be NULL</param>
        /// <returns></returns>
        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_GetDVRConfig(int lUserID, uint dwCommand, int lChannel, IntPtr lpOutBuffer, uint dwOutBufferSize, ref uint lpBytesReturned);

        /// <summary>
        /// Set the configuration information of the device.
        /// </summary>
        /// <param name="lUserID">Return value of login interface such as NET_DVR_Login_V40</param>
        /// <param name="dwCommand">Device configuration command, see "Remarks" for details</param>
        /// <param name="lChannel">Channel number, different commands correspond to different values, if this parameter is invalid, set it to 0xFFFFFFFF, see "Remarks" for details</param>
        /// <param name="lpInBuffer">Buffer pointer for input data.</param>
        /// <param name="dwInBufferSize">The buffer length of the input data (in bytes)</param>
        /// <returns></returns>
        /// <remarks>Different acquisition functions correspond to different structures and command numbers</remarks>
        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_SetDVRConfig(int lUserID, uint dwCommand, int lChannel, IntPtr lpInBuffer, uint dwInBufferSize);
    }
}