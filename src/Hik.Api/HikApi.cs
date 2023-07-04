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
    [ExcludeFromCodeCoverage]
    public class HikApi : IHikApi
    {
        private HikVideoService videoService;
        private HikPhotoService pictureService;
        private PlaybackService playbackService;

        public const string HCNetSDK = @"SDK\HCNetSDK.dll";
        public const string PlayCtrl = @"SDK\PlayCtrl.dll";

        public HikVideoService VideoService
        {
            get
            {
                return videoService ??= new HikVideoService();
            }
        }

        public HikPhotoService PhotoService
        {
            get
            {
                return pictureService ??= new HikPhotoService();
            }
        }

        public PlaybackService PlaybackService
        {
            get
            {
                return playbackService ??= new PlaybackService();
            }
        }

        public bool Initialize() => SdkHelper.InvokeSDK(() => NET_DVR_Init());

        public bool SetConnectTime(uint waitTimeMilliseconds, uint tryTimes)
            => SdkHelper.InvokeSDK(() => NET_DVR_SetConnectTime(waitTimeMilliseconds, tryTimes)); // 2000 , 1


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

        public Session Login(string ipAddress, int port, string userName, string password)
        {
            NET_DVR_DEVICEINFO_V30 deviceInfo = new NET_DVR_DEVICEINFO_V30();
            int userId = SdkHelper.InvokeSDK(() => NET_DVR_Login_V30(ipAddress, port, userName, password, ref deviceInfo));

            var ipChannels = InfoIPChannel(userId, deviceInfo);
            return new Session(userId, deviceInfo.byChanNum, ipChannels);
        }

        public void Cleanup()
        {
            SdkHelper.InvokeSDK(() => NET_DVR_Cleanup());
        }

        public void Logout(int userId)
        {
            SdkHelper.InvokeSDK(() => NET_DVR_Logout(userId));
        }

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

        public void SetTime(DateTime dateTime, int userId)
        {
            NET_DVR_TIME m_struTimeCfg = new NET_DVR_TIME(dateTime);
            int nSize = Marshal.SizeOf(m_struTimeCfg);
            IntPtr ptrTimeCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struTimeCfg, ptrTimeCfg, false);

            SdkHelper.InvokeSDK(() => NET_DVR_SetDVRConfig(userId, HikConst.NET_DVR_SET_TIMECFG, -1, ptrTimeCfg, (uint)nSize));
            Marshal.FreeHGlobal(ptrTimeCfg);
        }

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

            var dwAnalogChannelTotalNumber = deviceInfo.byChanNum;
            uint dwDigitalChannelTotalNumber = deviceInfo.byIPChanNum + 256 * (uint)deviceInfo.byHighDChanNum;

            if (dwDigitalChannelTotalNumber > 0)
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

                    for (int i = 0; i < dwAnalogChannelTotalNumber; i++)
                    {
                        var channel = ListAnalogChannel(i + 1, struIpParaCfgV40.byAnalogChanEnable[i]);
                        ipChannels.Add(channel);
                    }

                    byte byStreamType;
                    uint iDChanNum = 64;

                    if (dwDigitalChannelTotalNumber < 64)
                    {
                        //If the ip channels of device is less than 64,will get the real channel of device
                        iDChanNum = dwDigitalChannelTotalNumber;
                    }

                    for (int i = 0; i < iDChanNum; i++)
                    {
                        byStreamType = struIpParaCfgV40.struStreamMode[i].byGetStreamType;
                        var unionGetStream = struIpParaCfgV40.struStreamMode[i].uGetStream;

                        switch (byStreamType)
                        {
                            //At present NVR just support case 0-one way to get stream from device
                            case 0:
                                dwSize = Marshal.SizeOf(unionGetStream);
                                IntPtr ptrChanInfo = Marshal.AllocHGlobal(dwSize);
                                Marshal.StructureToPtr(unionGetStream, ptrChanInfo, false);
                                var struChanInfo = (NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(NET_DVR_IPCHANINFO));

                                //List ip channels
                                int channel = i + (int)struIpParaCfgV40.dwStartDChan;
                                ipChannels.Add(new IpChannel(channel, struChanInfo.byEnable, struChanInfo.byIPID) { Name = $"IP Channel {channel}" });
                                Marshal.FreeHGlobal(ptrChanInfo);
                                break;

                            default:
                                break;
                        }
                    }
                }
                Marshal.FreeHGlobal(ptrIpParaCfgV40);
            }
            return ipChannels;
        }

        public IpChannel ListAnalogChannel(int iChanNo, byte byEnable)
        {
            var str1 = string.Format("Camera {0}", iChanNo);

            return new IpChannel(iChanNo, byEnable, 0) { Name = str1 };//Add channels to list
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
        public static extern bool NET_DVR_SetDVRConfig(int lUserID, uint dwCommand, int lChannel, IntPtr lpInBuffer, uint dwInBufferSize);

    }
}