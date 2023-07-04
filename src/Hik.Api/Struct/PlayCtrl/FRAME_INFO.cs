namespace Hik.Api.Struct.PlayCtrl
{
    public struct FRAME_INFO
    {
        public int nWidth;
        public int nHeight;
        public int nStamp;
        public int nType;
        public int nFrameRate;
        public uint dwFrameNum;

        public void Init()
        {
            nWidth = 0;
            nHeight = 0;
            nStamp = 0;
            nType = 0;
            nFrameRate = 0;
            dwFrameNum = 0;
        }
    }
}
