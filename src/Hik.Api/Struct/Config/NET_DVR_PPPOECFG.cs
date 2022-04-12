using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Hik.Api.Struct.Config
{
    [ExcludeFromCodeCoverage]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct NET_DVR_PPPOECFG
    {
        public uint dwPPPOE;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.NAME_LEN, ArraySubType = UnmanagedType.I1)]
        public byte[] sPPPoEUser;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = HikConst.PASSWD_LEN)]
        public string sPPPoEPassword;

        public NET_DVR_IPADDR struPPPoEIP;
    }
}