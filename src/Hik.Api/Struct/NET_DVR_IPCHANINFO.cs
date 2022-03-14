using System.Runtime.InteropServices;

namespace Hik.Api.Struct
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NET_DVR_IPCHANINFO
    {
        public byte byEnable;
        public byte byIPID;
        public byte byChannel;
        public byte byIPIDHigh;
        public byte byTransProtocol;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 31, ArraySubType = UnmanagedType.I1)]
        public byte[] byRes;

        public void Init()
        {
            byRes = new byte[31];
        }
    }
}