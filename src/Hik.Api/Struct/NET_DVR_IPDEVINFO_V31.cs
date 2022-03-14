using System.Runtime.InteropServices;

namespace Hik.Api.Struct
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NET_DVR_IPDEVINFO_V31
    {
        public byte byEnable;
        public byte byProType;
        public byte byEnableQuickAdd;
        public byte byRes1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.NAME_LEN, ArraySubType = UnmanagedType.I1)]
        public byte[] sUserName;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.PASSWD_LEN, ArraySubType = UnmanagedType.I1)]
        public byte[] sPassword;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.MAX_DOMAIN_NAME, ArraySubType = UnmanagedType.I1)]
        public byte[] byDomain;

        internal NET_DVR_IPADDR struIP;
        public ushort wDVRPort;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 34, ArraySubType = UnmanagedType.I1)]
        public byte[] byRes2;

        public void Init()
        {
            sUserName = new byte[HikConst.NAME_LEN];
            sPassword = new byte[HikConst.PASSWD_LEN];
            byDomain = new byte[HikConst.MAX_DOMAIN_NAME];
            byRes2 = new byte[34];
        }
    }
}