using System;
using System.Runtime.InteropServices;

namespace Hik.Api.Struct.Video
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NET_DVR_PREVIEWINFO
    {
        public int lChannel;
        public uint dwStreamType; //0-main stream, 1-sub stream, 2-stream 3, 3-stream 4, and so on
        public uint dwLinkMode; //0- TCP mode, 1- UDP mode, 2- multicast mode, 3- RTP mode, 4-RTP/RTSP, 5-RSTP/HTTP
        public IntPtr hPlayWnd;
        public bool bBlocked;
        public bool bPassbackRecord; // Whether to enable video passback: 0-do not enable record passback, 1-enable record passback. ANR disconnection supplementary recording function, after the network abnormality between the client and the device is restored, the front-end data will be automatically synchronized, which requires the support of the device.
        public byte byPreviewMode; //delayed preview mode: 0- normal preview, 1- delayed preview

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.STREAM_ID_LEN, ArraySubType = UnmanagedType.I1)]
        public byte[] byStreamID;

        public byte byProtoType; //Application layer streaming protocol: 0- private protocol, 1- RTSP protocol. The streaming protocol supported by the main sub-stream can be known by logging in and returning the byMainProto and bySubProto values of the structure parameter NET_DVR_DEVICEINFO_V30. This parameter is valid only when the device supports both the private protocol and the RTSP protocol. The private protocol is used by default, and the RTSP protocol is optional.
        public byte byRes1;
        public byte byVideoCodingType;
        public uint dwDisplayBufNum;
        public byte byNPQMode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 215, ArraySubType = UnmanagedType.I1)]
        public byte[] byRes;
    }
}