using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Hik.Api.Struct.Config
{
    [ExcludeFromCodeCoverage]
    [StructLayout(LayoutKind.Sequential)]
    internal struct NET_DVR_DEVICECFG_V40
    {
        public uint dwSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.NAME_LEN, ArraySubType = UnmanagedType.I1)]
        public byte[] sDVRName;
        public uint dwDVRID;
        public uint dwRecycleRecord;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.SERIALNO_LEN, ArraySubType = UnmanagedType.I1)]
        public byte[] sSerialNumber;
        public uint dwSoftwareVersion;
        public uint dwSoftwareBuildDate;
        public uint dwDSPSoftwareVersion;
        public uint dwDSPSoftwareBuildDate;
        public uint dwPanelVersion;
        public uint dwHardwareVersion;
        public byte byAlarmInPortNum;
        public byte byAlarmOutPortNum;
        public byte byRS232Num;
        public byte byRS485Num;
        public byte byNetworkPortNum;
        public byte byDiskCtrlNum;
        public byte byDiskNum;
        public byte byDVRType;
        public byte byChanNum;
        public byte byStartChan;
        public byte byDecordChans;
        public byte byVGANum;
        public byte byUSBNum;
        public byte byAuxoutNum;
        public byte byAudioNum;
        public byte byIPChanNum;
        public byte byZeroChanNum;
        public byte bySupport;
        public byte byEsataUseage;
        public byte byIPCPlug;
        public byte byStorageMode;
        public byte bySupport1;
        public ushort wDevType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HikConst.DEV_TYPE_NAME_LEN, ArraySubType = UnmanagedType.I1)]
        public byte[] byDevTypeName;
        public byte bySupport2;
        public byte byAnalogAlarmInPortNum;
        public byte byStartAlarmInNo;
        public byte byStartAlarmOutNo;
        public byte byStartIPAlarmInNo;
        public byte byStartIPAlarmOutNo;
        public byte byHighIPChanNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9, ArraySubType = UnmanagedType.I1)]
        public byte[] byRes2;
    }
}
