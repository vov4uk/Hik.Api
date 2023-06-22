using Hik.Api.Abstraction;
using Hik.Api.Helpers;
using Hik.Api.Struct;
using Hik.Api.Struct.Video;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Hik.Api.Services
{
    public class HikVideoService : FileService
    {
        public event EventHandler<byte[]> RealDataReceived;
        public virtual int StartDownloadFile(int userId, string sourceFile, string destinationPath)
        {
            int downloadHandle = SdkHelper.InvokeSDK(() => NET_DVR_GetFileByName(userId, sourceFile, destinationPath));

            uint iOutValue = 0;
            SdkHelper.InvokeSDK(() => NET_DVR_PlayBackControl_V40(downloadHandle, HikConst.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue));
            return downloadHandle;
        }

        public virtual void StopDownloadFile(int fileHandle)
        {
            SdkHelper.InvokeSDK(() => NET_DVR_StopGetFile(fileHandle));
        }

        public virtual int GetDownloadPosition(int fileHandle)
        {
            return SdkHelper.InvokeSDK(() => NET_DVR_GetDownloadPos(fileHandle));
        }

        public int StartPlayBack(int userId, int channel)
        {
            NET_DVR_PREVIEWINFO lpPreviewInfo = new NET_DVR_PREVIEWINFO
            {
                lChannel = (short)channel,
                dwStreamType = 0,
                dwLinkMode = 0,
                bBlocked = true,
                dwDisplayBufNum = 1,
                byProtoType = 0,
                byPreviewMode = 0
            };

            REALDATACALLBACK RealData = new REALDATACALLBACK(RealDataCallBack);
            return SdkHelper.InvokeSDK(() => NET_DVR_RealPlay_V40(userId, ref lpPreviewInfo, RealData, new IntPtr()));
        }

        public bool StopPlayBack(int palybackId)
        {
            return SdkHelper.InvokeSDK(() => NET_DVR_StopRealPlay(palybackId));
        }

        public bool StartRecording(int playbackId, string filePath, int userId, int channel)
        {
            SdkHelper.InvokeSDK(() => NET_DVR_MakeKeyFrame(userId, channel));
            return SdkHelper.InvokeSDK(() => NET_DVR_SaveRealData(playbackId, filePath));
        }
        public bool StopRecording(int playbackId)
        {
            return SdkHelper.InvokeSDK(() => NET_DVR_StopSaveRealData(playbackId));
        }

        internal override int FindNext(int findId, ref ISourceFile source)
        {
            NET_DVR_FINDDATA_V30 findData = default(NET_DVR_FINDDATA_V30);
            int res = SdkHelper.InvokeSDK(() => NET_DVR_FindNextFile_V30(findId, ref findData));

            source = findData;
            return res;
        }

        protected override int StartFind(int userId, DateTime periodStart, DateTime periodEnd, int channel)
        {
            NET_DVR_FILECOND_V40 findConditions = new NET_DVR_FILECOND_V40
            {
                lChannel = channel,
                dwFileType = 0xff, //0xff-All，0-Timing record，1-Motion detection，2-Alarm trigger，...
                dwIsLocked = 0xff, //0-unfixed file，1-fixed file，0xff means all files (including fixed and unfixed files)
                struStartTime = new NET_DVR_TIME(periodStart),
                struStopTime = new NET_DVR_TIME(periodEnd),
            };

            return SdkHelper.InvokeSDK(() => NET_DVR_FindFile_V40(userId, ref findConditions));
        }

        protected override bool StopFind(int findId)
        {
            return SdkHelper.InvokeSDK(() => NET_DVR_FindClose_V30(findId));
        }

        private void RealDataCallBack(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr pUser)
        {
            if (dwBufSize > 0)
            {
                //try
                //{
                    byte[] sData = new byte[dwBufSize];
                    Marshal.Copy(pBuffer, sData, 0, (int)dwBufSize);

                    RealDataReceived?.Invoke(this, sData);

                    //string str = $"{DateTime.Now:yyyyMMdd_HHmmss}.ps";
                    //FileStream fs = new FileStream(str, FileMode.Append);
                    //int iLen = (int)dwBufSize;
                    //fs.Write(sData, 0, iLen);
                    //fs.Close();
                //}
                //catch (IOException)
                //{
                //}
            }
        }

        [DllImport(HikApi.DllPath)]
        private static extern bool NET_DVR_FindClose_V30(int lFindHandle);

        [DllImport(HikApi.DllPath)]
        private static extern int NET_DVR_FindNextFile_V30(int lFindHandle, ref NET_DVR_FINDDATA_V30 lpFindData);

        [DllImport(HikApi.DllPath)]
        private static extern int NET_DVR_FindFile_V40(int lUserID, ref NET_DVR_FILECOND_V40 pFindCond);

        [DllImport(HikApi.DllPath)]
        private static extern int NET_DVR_GetFileByName(int lUserID, string sDVRFileName, string sSavedFileName);

        [DllImport(HikApi.DllPath)]
        private static extern bool NET_DVR_PlayBackControl_V40(int lPlayHandle, uint dwControlCode, IntPtr lpInBuffer, uint dwInValue, IntPtr lpOutBuffer, ref uint lPOutValue);

        [DllImport(HikApi.DllPath)]
        private static extern int NET_DVR_GetDownloadPos(int lFileHandle);

        [DllImport(HikApi.DllPath)]
        private static extern bool NET_DVR_StopGetFile(int lFileHandle);

        [DllImport(HikApi.DllPath)]
        public static extern bool NET_DVR_MakeKeyFrame(int lUserID, int lChannel);

        [DllImport(HikApi.DllPath)]
        public static extern bool NET_DVR_SaveRealData(int lRealHandle, string sFileName);

        [DllImport(HikApi.DllPath)]
        public static extern bool NET_DVR_StopSaveRealData(int lRealHandle);

        [DllImport(HikApi.DllPath)]
        public static extern int NET_DVR_RealPlay_V40(int iUserID, ref NET_DVR_PREVIEWINFO lpPreviewInfo, REALDATACALLBACK fRealDataCallBack_V30, IntPtr pUser);

        public delegate void REALDATACALLBACK(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr pUser);

        [DllImport(HikApi.DllPath)]
        public static extern bool NET_DVR_StopRealPlay(int iRealHandle);
    }
}
