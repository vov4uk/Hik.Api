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
    public class VideoService : FileService, IVideoService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoService"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public VideoService(IHikApi session): base(session) { }

        /// <summary>
        /// Start download File
        /// </summary>
        /// <param name="sourceFile">Hik remote file name</param>
        /// <param name="destinationPath">Save path</param>
        /// <returns>Download handler</returns>
        public virtual int StartDownloadFile(string sourceFile, string destinationPath)
        {
            int downloadHandle = SdkHelper.InvokeSDK(() => NET_DVR_GetFileByName(session.UserId, sourceFile, destinationPath));

            uint iOutValue = 0;
            SdkHelper.InvokeSDK(() => NET_DVR_PlayBackControl_V40(downloadHandle, HikConst.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue));
            return downloadHandle;
        }

        /// <summary>
        /// Stop Download File
        /// </summary>
        /// <param name="fileHandle">Download handler</param>
        public virtual void StopDownloadFile(int fileHandle)
        {
            SdkHelper.InvokeSDK(() => NET_DVR_StopGetFile(fileHandle));
        }

        /// <summary>
        /// Get the progress of the current download video file
        /// </summary>
        /// <param name="fileHandle">Download handler</param>
        /// <returns></returns>
        public virtual int GetDownloadPosition(int fileHandle)
        {
            return SdkHelper.InvokeSDK(() => NET_DVR_GetDownloadPos(fileHandle));
        }

        /// <summary>
        ///Get the found file information one by one
        /// </summary>
        /// <param name="findId">The find identifier.</param>
        /// <param name="source">The source.</param>
        /// <returns>-1 means failure, and other values are used as parameters of functions such as NET_DVR_FindClose. </returns>
        internal override int FindNext(int findId, ref ISourceFile source)
        {
            NET_DVR_FINDDATA_V30 findData = default(NET_DVR_FINDDATA_V30);
            int res = SdkHelper.InvokeSDK(() => NET_DVR_FindNextFile_V30(findId, ref findData));

            source = findData;
            return res;
        }

        /// <summary>Starts the find.</summary>
        /// <param name="periodStart">The period start.</param>
        /// <param name="periodEnd">The period end.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>Download handler </returns>
        protected override int StartFind(DateTime periodStart, DateTime periodEnd, int channel)
        {
            NET_DVR_FILECOND_V40 findConditions = new NET_DVR_FILECOND_V40
            {
                lChannel = channel,
                dwFileType = 0xff, //0xff-All，0-Timing record，1-Motion detection，2-Alarm trigger，...
                dwIsLocked = 0xff, //0-unfixed file，1-fixed file，0xff means all files (including fixed and unfixed files)
                struStartTime = new NET_DVR_TIME(periodStart),
                struStopTime = new NET_DVR_TIME(periodEnd),
            };

            return SdkHelper.InvokeSDK(() => NET_DVR_FindFile_V40(session.UserId, ref findConditions));
        }

        /// <summary>Close the file search and release resources.</summary>
        /// <param name="findId">The find identifier.</param>
        /// <returns>TRUE means success, FALSE means failure.</returns>
        protected override bool StopFind(int findId) => SdkHelper.InvokeSDK(() => NET_DVR_FindClose_V30(findId));

        #region SDK

        /// <summary>
        /// This API is used to close file search and release the resource.
        /// </summary>
        /// <param name="lFindHandle">File search handle, return value of NET_DVR_FindFile_V40, NET_DVR_FindFileByEvent or NET_DVR_FindFile_V30</param>
        /// <returns>TRUE means success, FALSE means failure.</returns>
        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_FindClose_V30(int lFindHandle);

        /// <summary>
        /// Get the found file information one by one
        /// </summary>
        /// <param name="lFindHandle">File search handle, return value of NET_DVR_FindFile_V40 or NET_DVR_FindFile_V30</param>
        /// <param name="lpFindData">The pointer to save the file information</param>
        /// <returns>-1 indicates failure, and other values indicate the current acquisition status and other information. </returns>
        /// <remarks>Before calling this function, please call NET_DVR_FindFile_V30 to get current handle firstly. The interface only supports to get one file. We should call the interface repetitively to get all files. We can get other information, like card number and whether the file is locked,  by calling this API as well.</remarks>
        [DllImport(HikApi.HCNetSDK)]
        private static extern int NET_DVR_FindNextFile_V30(int lFindHandle, ref NET_DVR_FINDDATA_V30 lpFindData);

        /// <summary>
        /// Find device video files according to file type and time.
        /// </summary>
        /// <param name="lUserID">The return value of login interface such as NET_DVR_Login_V40</param>
        /// <param name="pFindCond">The file information structure to be found</param>
        /// <returns>-1 means failure, and other values are used as parameters of functions such as NET_DVR_FindClose. </returns>
        /// <remarks>This interface specifies the information of the video file to be found. After the call is successful, you can call the NET_DVR_FindNextFile_V40 interface to obtain the file information. </remarks>
        [DllImport(HikApi.HCNetSDK)]
        private static extern int NET_DVR_FindFile_V40(int lUserID, ref NET_DVR_FILECOND_V40 pFindCond);

        /// <summary>
        /// Download video file by file name
        /// </summary>
        /// <param name="lUserID">The return value of NET_DVR_Login or NET_DVR_Login_V30</param>
        /// <param name="sDVRFileName">The name of the video file to be downloaded, the length of the file name must be less than 100 bytes. </param>
        /// <param name="sSavedFileName">The file path saved to the PC after downloading, must be an absolute path (including the file name). </param>
        /// <returns>Return -1 if it is failed, and other values could be used as the parameter of functions NET_DVR_StopGetFile. Please call NET_DVR_GetLastError to get the error code. </returns>
        /// <remarks>Before calling this interface to download file, we can call the interface of searching record file to get file name. The interface have assigned the file to be downloaded currently. After calling it successfully, it needs to call starting play control command NET_DVR_PLAYSTART of NET_DVR_PlayBackControl_V40 to download file. </remarks>
        [DllImport(HikApi.HCNetSDK)]
        private static extern int NET_DVR_GetFileByName(int lUserID, string sDVRFileName, string sSavedFileName);

        /// <summary>
        /// This API is used to control playback status.
        /// </summary>
        /// <param name="lPlayHandle">play handle, return value of NET_DVR_PlayBackByName, NET_DVR_PlayBackReverseByName or NET_DVR_PlayBackByTime_V40, NET_DVR_PlayBackReverseByTime_V40. </param>
        /// <param name="dwControlCode">Control video playback status command</param>
        /// <param name="lpInBuffer">Pointer to the input parameter</param>
        /// <param name="dwInValue">Input parameter length. Not used, reserved. </param>
        /// <param name="lpOutBuffer">Pointer to the output parameter</param>
        /// <param name="lPOutValue">The length of the output parameter</param>
        /// <returns>TRUE means success, FALSE means failure.</returns>
        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_PlayBackControl_V40(int lPlayHandle, uint dwControlCode, IntPtr lpInBuffer, uint dwInValue, IntPtr lpOutBuffer, ref uint lPOutValue);

        /// <summary>
        /// Get the progress of the current download video file
        /// </summary>
        /// <param name="lFileHandle">Download handle, return value of NET_DVR_GetFileByName, NET_DVR_GetFileByTime_V40 or NET_DVR_GetFileByTime </param>
        /// <returns>-1 means failure; 0~100 means the progress of the download; 100 means the end of the download; the normal range is 0-100, if it returns 200, it means that there is a network exception. </returns>
        /// <remarks>This interface is used to obtain the download progress when downloading video files by file name. </remarks>

        [DllImport(HikApi.HCNetSDK)]
        private static extern int NET_DVR_GetDownloadPos(int lFileHandle);

        /// <summary>
        /// This API is used to stop downloading record files.
        /// </summary>
        /// <param name="lFileHandle">Download handle, return value of NET_DVR_GetFileByName, NET_DVR_GetFileByTime_V40 or NET_DVR_GetFileByTime. </param>
        /// <returns>TRUE means success, FALSE means failure.</returns>
        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_StopGetFile(int lFileHandle);

        #endregion
    }
}
