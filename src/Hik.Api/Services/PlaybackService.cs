using System.Runtime.InteropServices;
using System;
using Hik.Api.Helpers;
using Hik.Api.Struct.Video;

namespace Hik.Api.Services
{
    /// <summary>
    /// Playback Service
    /// </summary>
    public class PlaybackService
    {
        private HikApi session;

        internal PlaybackService(HikApi session)
        {
            this.session = session;
        }

        /// <summary>
        /// Start live preview without callback, all receiver live data will be handled by PictureBox Handle
        /// </summary>
        /// <param name="channel">channel.</param>
        /// <param name="playbackWindowHandler">System.Windows.Forms.PictureBox Handle</param>
        /// <returns></returns>
        public int StartPlayBack(int channel, IntPtr? playbackWindowHandler = null)
        {
            NET_DVR_PREVIEWINFO lpPreviewInfo = new NET_DVR_PREVIEWINFO
            {
                lChannel = (short)channel,
                dwStreamType = 0,
                dwLinkMode = 0,
                bBlocked = true,
                dwDisplayBufNum = 1,
                byProtoType = 0,
                byPreviewMode = 0,
                hPlayWnd = playbackWindowHandler ?? IntPtr.Zero
            };

            return SdkHelper.InvokeSDK(() => NET_DVR_RealPlay_V40(session.UserId, ref lpPreviewInfo, null, new IntPtr()));
        }

        /// <summary>
        /// Stop real-time preview
        /// </summary>
        /// <param name="playbackId">The playback identifier.</param>
        /// <returns>
        /// TRUE means success, FALSE means failure
        /// </returns>
        public bool StopPlayBack(int playbackId) => SdkHelper.InvokeSDK(() => NET_DVR_StopRealPlay(playbackId));

        /// <summary>
        /// Start recording live stream to filePath in .mp4 format
        /// </summary>
        /// <param name="playbackId">playback identifier.</param>
        /// <param name="filePath">file path.</param>
        /// <param name="channel">channel.</param>
        /// <returns></returns>
        public bool StartRecording(int playbackId, string filePath, int channel)
        {
            SdkHelper.InvokeSDK(() => NET_DVR_MakeKeyFrame(session.UserId, channel));
            return SdkHelper.InvokeSDK(() => NET_DVR_SaveRealData(playbackId, filePath));
        }

        /// <summary>
        /// Stop recording live stream to filePath
        /// </summary>
        /// <param name="playbackId">The playback identifier.</param>
        /// <returns></returns>
        public bool StopRecording(int playbackId)
        {
            return SdkHelper.InvokeSDK(() => NET_DVR_StopSaveRealData(playbackId));
        }

        #region SDK
        /// <summary>
        /// Make the main stream create a key frame(I frame)
        /// </summary>
        /// <param name="lUserID">The return value of NET_DVR_Login_V30</param>
        /// <param name="lChannel">Channel number.</param>
        /// <returns>Return TRUE on success, FALSE on failure. Please call NET_DVR_GetLastError to get the error code.</returns>
        /// <remarks>The interface is used to reset I frame, please call NET_DVR_MakeKeyFrame or NET_DVR_MakeKeyFrameSub to reset I frame for the main stream or sub stream according to the set preview parameter NET_DVR_CLIENTINFO.</remarks>
        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_MakeKeyFrame(int lUserID, int lChannel);

        /// <summary>
        /// Capture data and save to assigned file
        /// </summary>
        /// <param name="lRealHandle">The return value of NET_DVR_RealPlay_V30</param>
        /// <param name="sFileName">Pointer of file path</param>
        /// <returns>Return TRUE on success, FALSE on failure. Please call NET_DVR_GetLastError to get the error code</returns>
        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_SaveRealData(int lRealHandle, string sFileName);

        /// <summary>
        /// Stop save real data.
        /// </summary>
        /// <param name="lRealHandle">The return value of NET_DVR_RealPlay_V30</param>
        /// <returns>Return TRUE on success, FALSE on failure. Please call NET_DVR_GetLastError to get the error code.</returns>
        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_StopSaveRealData(int lRealHandle);

        /// <summary>
        /// Real-time preview.
        /// </summary>
        /// <param name="iUserID">The return value of NET_DVR_Login() or NET_DVR_Login_V30()</param>
        /// <param name="lpPreviewInfo">Preview parameters</param>
        /// <param name="fRealDataCallBack_V30">code stream data callback function</param>
        /// <param name="pUser">User data</param>
        /// <returns>1 means failure, other values are used as handle parameters of functions such as NET_DVR_StopRealPlay</returns>

        [DllImport(HikApi.HCNetSDK)]
        private static extern int NET_DVR_RealPlay_V40(int iUserID, ref NET_DVR_PREVIEWINFO lpPreviewInfo, REALDATACALLBACK fRealDataCallBack_V30, IntPtr pUser);

        /// <summary>
        /// preview callback
        /// </summary>
        /// <param name="lRealHandle">The current preview handle</param>
        /// <param name="dwDataType"> data type</param>
        /// <param name="pBuffer">pointer to the buffer where the data is stored.</param>
        /// <param name="dwBufSize">buffer size.</param>
        /// <param name="pUser">user data</param>
        private delegate void REALDATACALLBACK(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr pUser);

        /// <summary>
        /// Stop real-time preview
        /// </summary>
        /// <param name="iRealHandle">real-time preview handle</param>
        /// <returns>TRUE means success, FALSE means failure</returns>
        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_StopRealPlay(int iRealHandle);
        #endregion
    }
}
