using System.Runtime.InteropServices;

namespace Hik.Api.Struct.Config
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NET_DVR_ETHERNET_V30
    {
        public NET_DVR_IPADDR struDVRIP;
        public NET_DVR_IPADDR struDVRIPMask;
        public uint dwNetInterface;
        public ushort wDVRPort;
        public ushort wMTU;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.MACADDR_LEN, ArraySubType = UnmanagedType.I1)]
        public byte[] byMACAddr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.I1)]
        public byte[] byRes;
    }
}