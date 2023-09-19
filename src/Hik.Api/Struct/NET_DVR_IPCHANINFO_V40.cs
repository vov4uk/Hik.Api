using System.Runtime.InteropServices;

namespace Hik.Api.Struct
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NET_DVR_IPCHANINFO_V40
    {
        public byte byEnable;
        public byte byRes1;
        public ushort wIPID;
        public uint dwChannel;
        public byte byTransProtocol;
        public byte byTransMode;
        public byte byFactoryType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 241, ArraySubType = UnmanagedType.I1)]
        public byte[] byRes;
    }
}
