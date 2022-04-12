namespace Hik.Api
{
    internal static class HikConst
    {
        public const int CARDNUM_LEN_OUT = 32;
        public const int GUID_LEN = 16;
        public const int CARDNUM_LEN_V30 = 40;
        public const int SERIALNO_LEN = 48;
        public const int PICTURE_NAME_LEN = 64;
        public const int MAX_LICENSE_LEN = 16;
        public const int NET_DVR_PLAYSTART = 1;
        public const int NET_DVR_FILE_SUCCESS = 1000;
        public const int NET_DVR_ISFINDING = 1002;
        public const int NET_DVR_GET_IPPARACFG_V40 = 1062;

        public const int NET_DVR_GET_HDCFG = 1054;
        public const int MAX_DISKNUM_V30 = 33;

        public const int MAX_ANALOG_CHANNUM = 32;
        public const int MAX_IP_CHANNEL = 32;
        public const int MAX_IP_DEVICE_V40 = 64;
        public const int MAX_CHANNUM_V30 = MAX_ANALOG_CHANNUM + MAX_IP_CHANNEL;//64
        public const int MAX_DOMAIN_NAME = 64;
        public const int NAME_LEN = 32;
        public const int PASSWD_LEN = 16;
        public const int NET_DVR_GET_TIMECFG = 118;
        public const int NET_DVR_SET_TIMECFG = 119;
        public const int DEV_TYPE_NAME_LEN = 24;
        public const int NET_DVR_GET_DEVICECFG_V40 = 1100;
        public const int MAX_ETHERNET = 2;
        public const int MACADDR_LEN = 6;
        public const int NET_DVR_GET_NETCFG_V30 = 1000;
    }
}
