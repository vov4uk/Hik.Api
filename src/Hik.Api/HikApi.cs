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

        public const string DllPath = @"SDK\HCNetSDK";

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
            NET_DVR_DEVICEINFO_V30 deviceInfo = default;
            int userId = SdkHelper.InvokeSDK(() => NET_DVR_Login_V30(ipAddress, port, userName, password, ref deviceInfo));

            var ipChannels = InfoIPChannel(userId, deviceInfo);
            return new Session(userId, deviceInfo.byIPChanNum, ipChannels);
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
                //The demo just acquire 64 channels of first group.
                //If ip channels of device is more than 64,you should call NET_DVR_GET_IPPARACFG_V40 times to acquire more according to group 0~i

                var ipChannelsConfig = SdkHelper.InvokeSDK(() => NET_DVR_GetDVRConfig(userId, HikConst.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, (uint)dwSize, ref dwReturn));
                if (ipChannelsConfig)
                {
                    // succ
                    struIpParaCfgV40 = (NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIpParaCfgV40, typeof(NET_DVR_IPPARACFG_V40));

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
                                ipChannels.Add(new IpChannel(i + (int)struIpParaCfgV40.dwStartDChan, struChanInfo.byEnable, struChanInfo.byIPID));
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

        [DllImport(DllPath)]
        private static extern bool NET_DVR_Init();

        [DllImport(DllPath)]
        private static extern bool NET_DVR_SetLogToFile(int bLogEnable, string strLogDir, bool bAutoDel);

        [DllImport(DllPath)]
        private static extern int NET_DVR_Login_V30(string sDVRIP, int wDVRPort, string sUserName, string sPassword, ref NET_DVR_DEVICEINFO_V30 lpDeviceInfo);

        [DllImport(DllPath)]
        private static extern bool NET_DVR_Logout(int iUserID);

        [DllImport(DllPath)]
        private static extern bool NET_DVR_Cleanup();

        [DllImport(DllPath)]
        private static extern bool NET_DVR_SetConnectTime(uint dwWaitTime, uint dwTryTimes);

        [DllImport(DllPath)]
        private static extern bool NET_DVR_SetReconnect(uint dwInterval, int bEnableRecon);

        [DllImport(DllPath)]
        private static extern bool NET_DVR_GetDVRConfig(int lUserID, uint dwCommand, int lChannel, IntPtr lpOutBuffer, uint dwOutBufferSize, ref uint lpBytesReturned);
    }
}