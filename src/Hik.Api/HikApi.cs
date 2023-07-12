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
        /// NET_DVR_Init
        /// </summary>
        /// <returns></returns>
        public bool Initialize() => SdkHelper.InvokeSDK(() => NET_DVR_Init());

        /// <summary>
        /// NET_DVR_SetConnectTime
        /// </summary>
        /// <param name="waitTimeMilliseconds"></param>
        /// <param name="tryTimes"></param>
        /// <returns></returns>
        public bool SetConnectTime(uint waitTimeMilliseconds, uint tryTimes)
            => SdkHelper.InvokeSDK(() => NET_DVR_SetConnectTime(waitTimeMilliseconds, tryTimes)); // 2000 , 1

        /// <summary>
        /// NET_DVR_SetReconnect
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="enableRecon"></param>
        /// <returns></returns>
        public bool SetReconnect(uint interval, int enableRecon)
            => SdkHelper.InvokeSDK(() => NET_DVR_SetReconnect(interval, enableRecon)); // 10000 , 1

        /// <summary>Setups the logs.</summary>
        /// <param name="logLevel">Log level. 0- close log(default), 1- output ERROR log only, 2- output ERROR and DEBUG log, 3- output all log, including ERROR, DEBUG and INFO log</param>
        /// <param name="logDirectory">The log directory. Log file saving path, if set to NULL, the default path for Windows is "C:\\SdkLog\\", and the default path for Linux is ""/home/sdklog/"</param>
        /// <param name="autoDelete">Whether to delete the files which exceed the number limit. Default: TRUE.</param>
        /// <returns>bool</returns>
        public bool SetupLogs(int logLevel, string logDirectory, bool autoDelete)
        {
            return SdkHelper.InvokeSDK(() => NET_DVR_SetLogToFile(logLevel, logDirectory, autoDelete));
        }

        /// <summary>
        /// NET_DVR_Login_V30
        /// </summary>
        /// <param name="ipAddress">only ip addresess, host name not works</param>
        /// <param name="port">default 8000</param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>session</returns>
        public Session Login(string ipAddress, int port, string userName, string password)
        {
            NET_DVR_DEVICEINFO_V30 deviceInfo = new NET_DVR_DEVICEINFO_V30();
            int userId = SdkHelper.InvokeSDK(() => NET_DVR_Login_V30(ipAddress, port, userName, password, ref deviceInfo));

            var channels = InfoIPChannel(userId, deviceInfo);
            return new Session(userId, deviceInfo.byChanNum, channels);
        }

        /// <summary>
        /// NET_DVR_Cleanup
        /// </summary>
        public void Cleanup()
        {
            SdkHelper.InvokeSDK(() => NET_DVR_Cleanup());
        }

        /// <summary>
        /// NET_DVR_Logout
        /// </summary>
        /// <param name="userId"></param>
        public void Logout(int userId)
        {
            SdkHelper.InvokeSDK(() => NET_DVR_Logout(userId));
        }

        /// <summary>
        /// Get SD Card info, capaity, free space, status etc.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public HdInfo GetHddStatus(int userId)
        {
            NET_DVR_HDCFG hdConfig = default;
            uint returned = 0;
            int sizeOfConfig = Marshal.SizeOf(hdConfig);
            IntPtr ptrDeviceCfg = Marshal.AllocHGlobal(sizeOfConfig);
            Marshal.StructureToPtr(hdConfig, ptrDeviceCfg, false);
            SdkHelper.InvokeSDK(() => NET_DVR_GetDVRConfig(
                userId,
                HikConst.NET_DVR_GET_HDCFG,
                -1,
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
        /// <param name="userId"></param>
        /// <returns></returns>
        public DateTime GetTime(int userId)
        {
            NET_DVR_TIME m_struTimeCfg = default;

            uint dwReturn = 0;
            int nSize = Marshal.SizeOf(m_struTimeCfg);
            IntPtr ptrTimeCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struTimeCfg, ptrTimeCfg, false);

            SdkHelper.InvokeSDK(() => NET_DVR_GetDVRConfig(userId, HikConst.NET_DVR_GET_TIMECFG, 1, ptrTimeCfg, (uint)nSize, ref dwReturn));

            m_struTimeCfg = (NET_DVR_TIME)Marshal.PtrToStructure(ptrTimeCfg, typeof(NET_DVR_TIME));

            Marshal.FreeHGlobal(ptrTimeCfg);

            return m_struTimeCfg.ToDateTime();
        }

        /// <summary>
        /// Set device current time
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="userId"></param>
        public void SetTime(DateTime dateTime, int userId)
        {
            NET_DVR_TIME m_struTimeCfg = new NET_DVR_TIME(dateTime);
            int nSize = Marshal.SizeOf(m_struTimeCfg);
            IntPtr ptrTimeCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struTimeCfg, ptrTimeCfg, false);

            SdkHelper.InvokeSDK(() => NET_DVR_SetDVRConfig(userId, HikConst.NET_DVR_SET_TIMECFG, -1, ptrTimeCfg, (uint)nSize));
            Marshal.FreeHGlobal(ptrTimeCfg);
        }

        /// <summary>
        /// Get Device Config
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
        /// Get Network Config
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
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

            dwAChanTotalNum = (uint)deviceInfo.byChanNum;
            dwDChanTotalNum = (uint)deviceInfo.byIPChanNum + 256 * (uint)deviceInfo.byHighDChanNum;
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
                    // succ
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
                            //目前NVR仅支持直接从设备取流 NVR supports only the mode: get stream from device directly
                            case 0:
                                IntPtr ptrChanInfo = Marshal.AllocHGlobal(dwSize);
                                Marshal.StructureToPtr(struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfo, false);
                                struChanInfo = (NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(NET_DVR_IPCHANINFO));

                                //列出IP通道 List the IP channel
                                var ipChannelNumber = struChanInfo.byIPID + struChanInfo.byIPIDHigh * 256 - iGroupNo * 64 - 1;
                                ipChannels.Add(ListIPChannel(ipChannelNumber, struChanInfo.byEnable, struChanInfo.byIPID));

                                Marshal.FreeHGlobal(ptrChanInfo);
                                break;
                            case 6:
                                IntPtr ptrChanInfoV40 = Marshal.AllocHGlobal(dwSize);
                                Marshal.StructureToPtr(struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfoV40, false);
                                struChanInfoV40 = (NET_DVR_IPCHANINFO_V40)Marshal.PtrToStructure(ptrChanInfoV40, typeof(NET_DVR_IPCHANINFO_V40));

                                //列出IP通道 List the IP channel
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

        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_Init();

        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_SetLogToFile(int bLogEnable, string strLogDir, bool bAutoDel);

        [DllImport(HCNetSDK)]
        private static extern int NET_DVR_Login_V30(string sDVRIP, int wDVRPort, string sUserName, string sPassword, ref NET_DVR_DEVICEINFO_V30 lpDeviceInfo);

        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_Logout(int iUserID);

        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_Cleanup();

        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_SetConnectTime(uint dwWaitTime, uint dwTryTimes);

        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_SetReconnect(uint dwInterval, int bEnableRecon);

        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_GetDVRConfig(int lUserID, uint dwCommand, int lChannel, IntPtr lpOutBuffer, uint dwOutBufferSize, ref uint lpBytesReturned);

        [DllImport(HCNetSDK)]
        private static extern bool NET_DVR_SetDVRConfig(int lUserID, uint dwCommand, int lChannel, IntPtr lpInBuffer, uint dwInBufferSize);
    }
}