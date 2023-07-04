using Hik.Api.Abstraction;
using Hik.Api.Helpers;
using Hik.Api.Struct;
using Hik.Api.Struct.Video;
using System;
using System.Runtime.InteropServices;

namespace Hik.Api.Services
{
    /// <summary>
    /// Video service
    /// </summary>
    public class HikVideoService : FileService
    {
        /// <summary>
        /// Start Download File
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sourceFile"></param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        public virtual int StartDownloadFile(int userId, string sourceFile, string destinationPath)
        {
            int downloadHandle = SdkHelper.InvokeSDK(() => NET_DVR_GetFileByName(userId, sourceFile, destinationPath));

            uint iOutValue = 0;
            SdkHelper.InvokeSDK(() => NET_DVR_PlayBackControl_V40(downloadHandle, HikConst.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue));
            return downloadHandle;
        }

        /// <summary>
        /// Stop Download File
        /// </summary>
        /// <param name="fileHandle"></param>
        public virtual void StopDownloadFile(int fileHandle)
        {
            SdkHelper.InvokeSDK(() => NET_DVR_StopGetFile(fileHandle));
        }

        /// <summary>
        ///  Return current progress
        /// </summary>
        /// <param name="fileHandle"></param>
        /// <returns></returns>
        public virtual int GetDownloadPosition(int fileHandle)
        {
            return SdkHelper.InvokeSDK(() => NET_DVR_GetDownloadPos(fileHandle));
        }

        internal override int FindNext(int findId, ref ISourceFile source)
        {
            NET_DVR_FINDDATA_V30 findData = default(NET_DVR_FINDDATA_V30);
            int res = SdkHelper.InvokeSDK(() => NET_DVR_FindNextFile_V30(findId, ref findData));

            source = findData;
            return res;
        }

        /// <summary>Starts the find.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="periodStart">The period start.</param>
        /// <param name="periodEnd">The period end.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>
        ///   <br />
        /// </returns>
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

        /// <summary>Stops the find.</summary>
        /// <param name="findId">The find identifier.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        protected override bool StopFind(int findId)
        {
            return SdkHelper.InvokeSDK(() => NET_DVR_FindClose_V30(findId));
        }

        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_FindClose_V30(int lFindHandle);

        [DllImport(HikApi.HCNetSDK)]
        private static extern int NET_DVR_FindNextFile_V30(int lFindHandle, ref NET_DVR_FINDDATA_V30 lpFindData);

        [DllImport(HikApi.HCNetSDK)]
        private static extern int NET_DVR_FindFile_V40(int lUserID, ref NET_DVR_FILECOND_V40 pFindCond);

        [DllImport(HikApi.HCNetSDK)]
        private static extern int NET_DVR_GetFileByName(int lUserID, string sDVRFileName, string sSavedFileName);

        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_PlayBackControl_V40(int lPlayHandle, uint dwControlCode, IntPtr lpInBuffer, uint dwInValue, IntPtr lpOutBuffer, ref uint lPOutValue);

        [DllImport(HikApi.HCNetSDK)]
        private static extern int NET_DVR_GetDownloadPos(int lFileHandle);

        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_StopGetFile(int lFileHandle);
    }
}
