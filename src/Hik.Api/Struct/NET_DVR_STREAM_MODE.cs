using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Hik.Api.Struct
{
    [ExcludeFromCodeCoverage]
    [StructLayout(LayoutKind.Sequential)]
    internal struct NET_DVR_STREAM_MODE
    {
        public byte byGetStreamType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.I1)]
        public byte[] byRes;

        internal NET_DVR_GET_STREAM_UNION uGetStream;

        public void Init()
        {
            byGetStreamType = 0;
            byRes = new byte[3];
        }
    }
}