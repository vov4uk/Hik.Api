using System.Runtime.InteropServices;

namespace Hik.Api.Struct
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct NET_DVR_GET_STREAM_UNION
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 492, ArraySubType = UnmanagedType.I1)]
        public byte[] byUnion;
        public void Init()
        {
            byUnion = new byte[492];
        }
    }
}
