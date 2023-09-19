using Hik.Api.Abstraction;
using Hik.Api.Data;
using Hik.Api.Helpers;
using Hik.Api.Struct;
using Hik.Api.Struct.Photo;
using System;
using System.Runtime.InteropServices;


namespace Hik.Api.Services
{
    /// <summary>
    /// Photo service
    /// </summary>
    public class HikPhotoService : FileService
    {
        /// <summary>
        /// Download File
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="remoteFileName">remote file name</param>
        /// <param name="size">Remote file size</param>
        /// <param name="destinationPath">Save path</param>
        public virtual void DownloadFile(int userId, string remoteFileName, long size, string destinationPath)
        {
            if (size > 0)
            {
                NET_DVR_PIC_PARAM temp = new NET_DVR_PIC_PARAM
                {
                    pDVRFileName = remoteFileName,
                    pSavedFileBuf = Marshal.AllocHGlobal((int)size),
                    dwBufLen = (uint)size
                };

                if (SdkHelper.InvokeSDK(() => NET_DVR_GetPicture_V50(userId, ref temp)))
                {
                    SdkHelper.InvokeSDK(() => NET_DVR_GetPicture(userId, temp.pDVRFileName, destinationPath));
                }

                Marshal.FreeHGlobal(temp.pSavedFileBuf);
            }
        }

        /// <summary>
        /// Download file
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="photo">Hik remote file</param>
        /// <param name="destinationPath">Save path</param>
        public virtual void DownloadFile(int userId, HikRemoteFile photo, string destinationPath)
        {
            DownloadFile(userId, photo.Name, photo.Size, destinationPath);
        }

        /// <summary>Stops the find.</summary>
        /// <param name="findId">The find identifier.</param>
        /// <returns>Success</returns>
        protected override bool StopFind(int findId)
        {
            return SdkHelper.InvokeSDK(() => NET_DVR_CloseFindPicture(findId));
        }

        internal override int FindNext(int findId, ref ISourceFile source)
        {
            NET_DVR_FIND_PICTURE_V50 findData = new NET_DVR_FIND_PICTURE_V50();

            int res = SdkHelper.InvokeSDK(() => NET_DVR_FindNextPicture_V50(findId, ref findData));
            source = findData;

            return res;
        }

        /// <summary>Starts the find.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="periodStart">The period start.</param>
        /// <param name="periodEnd">The period end.</param>
        /// <param name="channel">The channel.</param>
        /// <returns>Find identifier</returns>
        protected override int StartFind(int userId, DateTime periodStart, DateTime periodEnd, int channel)
        {

            NET_DVR_FIND_PICTURE_PARAM findConditions = new NET_DVR_FIND_PICTURE_PARAM
            {
                lChannel = channel,
                byFileType = 0xff, // all
                struStartTime = new NET_DVR_TIME(periodStart),
                struStopTime = new NET_DVR_TIME(periodEnd)
            };

            return SdkHelper.InvokeSDK(() => NET_DVR_FindPicture(userId, ref findConditions));
        }

        /// <summary>
        /// This API is used to get picture one by one.
        /// </summary>
        /// <param name="lUserID">[in] Handle of file searching, the return value of NET_DVR_FindPicture </param>
        /// <param name="pFindParam">Pointer for saving picture information </param>
        /// <returns>Return -1 if it is failed, and the other values stand for current status or other information. Please call NET_DVR_GetLastError to get the error code.</returns>
        /// <remarks>Before calling this function, please call NET_DVR_FindPicture to get current handle firstly. The interface only supports to get one picture. We should call the interface repetitively to get all pictures.</remarks>
        [DllImport(HikApi.HCNetSDK)]
        private static extern int NET_DVR_FindPicture(int lUserID, ref NET_DVR_FIND_PICTURE_PARAM pFindParam);

        /// <summary>
        /// Get picture information one by one in search result.
        /// </summary>
        /// <param name="lFindHandle">[in] Handle of finding picture, the return value of NET_DVR_FindPicture </param>
        /// <param name="lpFindData">[out] Save compass for picture information. </param>
        /// <returns>Return -1 for failure, other values indicate the current getting status. When -1 is returned, callNET_DVR_GetLastError to get the error code.</returns>
        /// <remarks>Before calling this API to get the searched picture information, call NET_DVR_FindPicture to get the current searching handle. This API is used to get one searched picture information. To get all the searched pictures information, you should call this API in loop.</remarks>
        [DllImport(HikApi.HCNetSDK)]
        private static extern int NET_DVR_FindNextPicture_V50(int lFindHandle, ref NET_DVR_FIND_PICTURE_V50 lpFindData);

        /// <summary>
        /// This API is used to close finding picture and release resource.
        /// </summary>
        /// <param name="lpFindHandle">[in] Handle of finding picture, the return value of NET_DVR_FindPicture </param>
        /// <returns>Returns TRUE on success, FALSE on failure. Please call NET_DVR_GetLastError to get the error code.</returns>
        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_CloseFindPicture(int lpFindHandle);

        /// <summary>
        /// This API is used to get picture data and save it in specified memory space.
        /// </summary>
        /// <param name="lUserID">User ID, the return value of NET_DVR_Login_V40 </param>
        /// <param name="lpPicParam">Return temprorary file</param>
        /// <returns>Returns TRUE for success, and FALSE for failure. When FALSE is returned, call NET_DVR_GetLastError to get the error code.</returns>
        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_GetPicture_V50(int lUserID, ref NET_DVR_PIC_PARAM lpPicParam);

        /// <summary>
        /// Nets the DVR get picture.
        /// </summary>
        /// <param name="lUserID">User ID, the returned value of API NET_DVR_Login_V40 </param>
        /// <param name="sDVRFileName">in] Name of picture to download</param>
        /// <param name="sSavedFileName">[in] Saving path (including file name) for downloaded pictures</param>
        /// <returns>Return TRUE for success, and return FALSE for failure. If API returns FALSE, call NET_DVR_GetLastError to get error code.</returns>
        /// <remarks>The picture format is JPEG, and the postfix of file name is ".jpg".</remarks>
        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_GetPicture(int lUserID, string sDVRFileName, [In] [MarshalAs(UnmanagedType.LPStr)] string sSavedFileName);
    }
}
