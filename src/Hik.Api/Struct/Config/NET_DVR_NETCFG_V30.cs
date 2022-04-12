using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Hik.Api.Struct.Config
{
    [ExcludeFromCodeCoverage]
    [StructLayout(LayoutKind.Sequential)]
    internal struct NET_DVR_NETCFG_V30
    {
        public uint dwSize;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.MAX_ETHERNET, ArraySubType = UnmanagedType.Struct)]
        public NET_DVR_ETHERNET_V30[] struEtherNet;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.Struct)]
        public NET_DVR_IPADDR[] struRes1;

        public NET_DVR_IPADDR struAlarmHostIpAddr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.I1)]
        public byte[] byRes2;

        public ushort wAlarmHostIpPort;
        public byte byUseDhcp;
        public byte byRes3;
        public NET_DVR_IPADDR struDnsServer1IpAddr;
        public NET_DVR_IPADDR struDnsServer2IpAddr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.MAX_DOMAIN_NAME, ArraySubType = UnmanagedType.I1)]
        public byte[] byIpResolver;

        public ushort wIpResolverPort;
        public ushort wHttpPortNo;
        public NET_DVR_IPADDR struMulticastIpAddr;
        public NET_DVR_IPADDR struGatewayIpAddr;
        public NET_DVR_PPPOECFG struPPPoE;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64, ArraySubType = UnmanagedType.I1)]
        public byte[] byRes;
    }
}