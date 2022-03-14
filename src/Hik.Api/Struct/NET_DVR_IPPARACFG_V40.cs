using System.Runtime.InteropServices;

namespace Hik.Api.Struct
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NET_DVR_IPPARACFG_V40
    {
        public uint dwSize;
        public uint dwGroupNum;
        public uint dwAChanNum;
        public uint dwDChanNum;
        public uint dwStartDChan;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.MAX_CHANNUM_V30, ArraySubType = UnmanagedType.I1)]
        public byte[] byAnalogChanEnable;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.MAX_IP_DEVICE_V40)]
        public NET_DVR_IPDEVINFO_V31[] struIPDevInfo;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.MAX_CHANNUM_V30)]
        public NET_DVR_STREAM_MODE[] struStreamMode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.I1)]
        public byte[] byRes2;
    }
}