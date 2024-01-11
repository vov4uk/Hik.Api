using Hik.Api.Abstraction;
using Hik.Api.Data;
using Hik.Api.Helpers;
using Hik.Api.Struct;
using Hik.Api.Struct.Config;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Hik.Api.Services
{
    /// <summary>
    /// Config service
    /// </summary>
    public class ConfigService: IConfigService
    {
        private readonly IHikApi session;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigService"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public ConfigService(IHikApi session)
        {
            this.session = session;
        }

        /// <summary>
        /// Get device current time
        /// </summary>
        /// <param name="ipChannel">Default value 1</param>
        /// <returns></returns>
        public DateTime GetTime(int ipChannel = 1)
        {
            NET_DVR_TIME timeCfg = default;

            uint dwReturn = 0;
            int nSize = Marshal.SizeOf(timeCfg);
            IntPtr ptrTimeCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(timeCfg, ptrTimeCfg, false);

            SdkHelper.InvokeSDK(() => NET_DVR_GetDVRConfig(session.UserId, HikConst.NET_DVR_GET_TIMECFG, ipChannel, ptrTimeCfg, (uint)nSize, ref dwReturn));

            timeCfg = (NET_DVR_TIME)Marshal.PtrToStructure(ptrTimeCfg, typeof(NET_DVR_TIME));

            Marshal.FreeHGlobal(ptrTimeCfg);

            return timeCfg.ToDateTime();
        }

        /// <summary>
        /// Set device current time
        /// </summary>
        /// <param name="dateTime">The date time.</param>
         /// <param name="ipChannel">Default value -1</param>
        public void SetTime(DateTime dateTime,int ipChannel = -1)
        {
            NET_DVR_TIME timeCfg = new NET_DVR_TIME(dateTime);
            int nSize = Marshal.SizeOf(timeCfg);
            IntPtr ptrTimeCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(timeCfg, ptrTimeCfg, false);

            SdkHelper.InvokeSDK(() => NET_DVR_SetDVRConfig(session.UserId, HikConst.NET_DVR_SET_TIMECFG, ipChannel, ptrTimeCfg, (uint)nSize));
            Marshal.FreeHGlobal(ptrTimeCfg);
        }

        /// <summary>
        /// Get device configuration information.
        /// </summary>
        /// <returns></returns>
        public DeviceConfig GetDeviceConfig()
        {
            NET_DVR_DEVICECFG_V40 deviceCfg = default;

            uint dwReturn = 0;
            int nSize = Marshal.SizeOf(deviceCfg);
            IntPtr ptrDeviceCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(deviceCfg, ptrDeviceCfg, false);

            SdkHelper.InvokeSDK(() => NET_DVR_GetDVRConfig(session.UserId, HikConst.NET_DVR_GET_DEVICECFG_V40, -1, ptrDeviceCfg, (uint)nSize, ref dwReturn));

            deviceCfg = (NET_DVR_DEVICECFG_V40)Marshal.PtrToStructure(ptrDeviceCfg, typeof(NET_DVR_DEVICECFG_V40));

            uint iVer1 = (deviceCfg.dwSoftwareVersion >> 24) & 0xFF;
            uint iVer2 = (deviceCfg.dwSoftwareVersion >> 16) & 0xFF;
            uint iVer3 = deviceCfg.dwSoftwareVersion & 0xFFFF;
            uint iVer4 = (deviceCfg.dwSoftwareBuildDate >> 16) & 0xFFFF;
            uint iVer5 = (deviceCfg.dwSoftwareBuildDate >> 8) & 0xFF;
            uint iVer6 = deviceCfg.dwSoftwareBuildDate & 0xFF;

            var deviceConfig = new DeviceConfig
            {
                Name = GetString(deviceCfg.sDVRName),
                TypeName = GetString(deviceCfg.byDevTypeName),
                AnalogChannel = Convert.ToInt32(deviceCfg.byChanNum),
                IPChannel = Convert.ToInt32(deviceCfg.byIPChanNum + 256 * deviceCfg.byHighIPChanNum),
                ZeroChannel = Convert.ToInt32(deviceCfg.byZeroChanNum),
                NetworkPort = Convert.ToInt32(deviceCfg.byNetworkPortNum),
                AlarmInPort = Convert.ToInt32(deviceCfg.byAlarmInPortNum),
                AlarmOutPort = Convert.ToInt32(deviceCfg.byAlarmOutPortNum),
                Serial = GetString(deviceCfg.sSerialNumber),
                Version = $"V{iVer1}.{iVer2}.{iVer3} Build {iVer4,0:D2}{iVer5,0:D2}{iVer6,0:D2}"
            };

            Marshal.FreeHGlobal(ptrDeviceCfg);
            return deviceConfig;
        }

        /// <summary>
        /// Get device network information.
        /// </summary>
        /// <returns>
        /// Network information
        /// </returns>
        public NetworkConfig GetNetworkConfig()
        {
            NET_DVR_NETCFG_V30 netCfg = default;
            uint dwReturn = 0;
            int nSize = Marshal.SizeOf(netCfg);
            IntPtr ptrNetCfg = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(netCfg, ptrNetCfg, false);

            SdkHelper.InvokeSDK(() => NET_DVR_GetDVRConfig(session.UserId, HikConst.NET_DVR_GET_NETCFG_V30, -1, ptrNetCfg, (uint)nSize, ref dwReturn));
            netCfg = (NET_DVR_NETCFG_V30)Marshal.PtrToStructure(ptrNetCfg, typeof(NET_DVR_NETCFG_V30));

            NetworkConfig networkConfig = new NetworkConfig
            {
                IPAddress = netCfg.struEtherNet[0].struDVRIP.sIpV4,
                GateWay = netCfg.struGatewayIpAddr.sIpV4,
                SubMask = netCfg.struEtherNet[0].struDVRIPMask.sIpV4,
                Dns = netCfg.struDnsServer1IpAddr.sIpV4,
                HostIP = netCfg.struAlarmHostIpAddr.sIpV4,
                AlarmHostIpPort = Convert.ToInt32(netCfg.wAlarmHostIpPort),
                HttpPort = Convert.ToInt32(netCfg.wHttpPortNo),
                DVRPort = Convert.ToInt32(netCfg.struEtherNet[0].wDVRPort),
                DHCP = netCfg.byUseDhcp == 1,
                PPPoE = netCfg.struPPPoE.dwPPPOE == 1,
                PPPoEName = GetString(netCfg.struPPPoE.sPPPoEUser),
                PPPoEPassword = netCfg.struPPPoE.sPPPoEPassword
            };

            Marshal.FreeHGlobal(ptrNetCfg);
            return networkConfig;
        }

        /// <summary>
        /// Get SD Card info, capacity, free space, status etc.
        /// </summary>
        /// <param name="ipChannel">Default value -1</param>
        /// <returns>HdInfo</returns>
        public HdInfo GetHddStatus(int ipChannel = -1)
        {
            NET_DVR_HDCFG hdConfig = default;
            uint returned = 0;
            int sizeOfConfig = Marshal.SizeOf(hdConfig);
            IntPtr ptrDeviceCfg = Marshal.AllocHGlobal(sizeOfConfig);
            Marshal.StructureToPtr(hdConfig, ptrDeviceCfg, false);
            SdkHelper.InvokeSDK(() => NET_DVR_GetDVRConfig(
                session.UserId,
                HikConst.NET_DVR_GET_HDCFG,
                ipChannel,
                ptrDeviceCfg,
                (uint)sizeOfConfig,
                ref returned));

            hdConfig = (NET_DVR_HDCFG)Marshal.PtrToStructure(ptrDeviceCfg, typeof(NET_DVR_HDCFG));
            Marshal.FreeHGlobal(ptrDeviceCfg);
            return new HdInfo(hdConfig.struHDInfo[0]);
        }

        internal static string GetString(byte[] arr)
        {
            return Encoding.UTF8.GetString(arr).Trim('\0');
        }

        #region SDK
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
        [DllImport(HikApi.HCNetSDK)]
        internal static extern bool NET_DVR_GetDVRConfig(int lUserID, uint dwCommand, int lChannel, IntPtr lpOutBuffer, uint dwOutBufferSize, ref uint lpBytesReturned);

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
        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_SetDVRConfig(int lUserID, uint dwCommand, int lChannel, IntPtr lpInBuffer, uint dwInBufferSize);

        #endregion
    }
}
