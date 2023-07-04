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
        /// <summary>
        /// Start live preview with callback, all receiver live data will be handled by PictureBox Handle
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="playbackWindowHandler">System.Windows.Forms.PictureBox Handle</param>
        /// <returns></returns>
        public int StartPlayBack(int userId, int channel, IntPtr? playbackWindowHandler = null)
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

            return SdkHelper.InvokeSDK(() => NET_DVR_RealPlay_V40(userId, ref lpPreviewInfo, null, new IntPtr()));
        }

        /// <summary>
        /// Stop Real Play
        /// </summary>
        /// <param name="palybackId"></param>
        /// <returns></returns>
        public bool StopPlayBack(int palybackId)
        {
            return SdkHelper.InvokeSDK(() => NET_DVR_StopRealPlay(palybackId));
        }

        /// <summary>
        /// Start recording live stream to filePath in .mp4 format
        /// </summary>
        /// <param name="playbackId"></param>
        /// <param name="filePath"></param>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool StartRecording(int playbackId, string filePath, int userId, int channel)
        {
            SdkHelper.InvokeSDK(() => NET_DVR_MakeKeyFrame(userId, channel));
            return SdkHelper.InvokeSDK(() => NET_DVR_SaveRealData(playbackId, filePath));
        }

        /// <summary>
        /// Stop recording live stream to filePath
        /// </summary>
        /// <param name="playbackId"></param>
        /// <returns></returns>
        public bool StopRecording(int playbackId)
        {
            return SdkHelper.InvokeSDK(() => NET_DVR_StopSaveRealData(playbackId));
        }

        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_MakeKeyFrame(int lUserID, int lChannel);

        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_SaveRealData(int lRealHandle, string sFileName);

        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_StopSaveRealData(int lRealHandle);

        [DllImport(HikApi.HCNetSDK)]
        private static extern int NET_DVR_RealPlay_V40(int iUserID, ref NET_DVR_PREVIEWINFO lpPreviewInfo, REALDATACALLBACK fRealDataCallBack_V30, IntPtr pUser);

        private delegate void REALDATACALLBACK(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr pUser);

        [DllImport(HikApi.HCNetSDK)]
        private static extern bool NET_DVR_StopRealPlay(int iRealHandle);
    }
}
