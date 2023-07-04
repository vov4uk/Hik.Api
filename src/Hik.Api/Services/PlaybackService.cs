using Hik.Api.Struct.PlayCtrl;
using System.Runtime.InteropServices;
using System;
using Hik.Api.Helpers;
using Hik.Api.Struct.Video;
using System.Threading;

namespace Hik.Api.Services
{
    public class PlaybackService
    {
        private int m_lPort = -1;
        private DECCBFUN m_fDisplayFun = null;

        public event EventHandler<byte[]> RealDataReceived;

        /// <summary>
        /// Start live preview with callback, all receiver live data will be handled by RealDataReceived event
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
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
                byPreviewMode = 0,
                hPlayWnd = IntPtr.Zero
            };

            REALDATACALLBACK RealData = new REALDATACALLBACK(RealDataCallBack);
            return SdkHelper.InvokeSDK(() => NET_DVR_RealPlay_V40(userId, ref lpPreviewInfo, RealData, new IntPtr()));
        }

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

        private void RealDataCallBack(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr pUser)
        {
            switch (dwDataType)
            {
                case NET_DVR_SYSHEAD:// sys head
                    if (dwBufSize > 0)
                    {
                        if (m_lPort >= 0)
                        {
                            return; //同一路码流不需要多次调用开流接口
                        }

                        //Get the port to play
                        PlayHelper.Invoke(m_lPort, () => PlayM4_GetPort(ref m_lPort));

                        //Set the stream mode: real-time stream mode
                        PlayHelper.Invoke(m_lPort, () => PlayM4_SetStreamOpenMode(m_lPort, STREAME_REALTIME));

                        //Open stream
                        PlayHelper.Invoke(m_lPort, () => PlayM4_OpenStream(m_lPort, pBuffer, dwBufSize, 2 * 1024 * 1024));

                        //Set the display buffer number
                        PlayHelper.Invoke(m_lPort, () => PlayM4_SetDisplayBuf(m_lPort, 15));

                        //Set the display mode
                        PlayHelper.Invoke(m_lPort, () => PlayM4_SetOverlayMode(m_lPort, 0, 0));

                        //Set callback function of decoded data
                        m_fDisplayFun = new DECCBFUN(DecCallbackFUN);
                        PlayHelper.Invoke(m_lPort, () => PlayM4_SetDecCallBackEx(m_lPort, m_fDisplayFun, IntPtr.Zero, 0));

                        //Start to play
                        PlayHelper.Invoke(m_lPort, () => PlayM4_Play(m_lPort, IntPtr.Zero));      // m_ptrRealHandle))    //live view window
                    }
                    break;
                case NET_DVR_STREAMDATA:     // video stream data
                    if (dwBufSize > 0 && m_lPort != -1)
                    {
                        for (int i = 0; i < 999; i++)
                        {
                            //Input the stream data to decode
                            if (!PlayM4_InputData(m_lPort, pBuffer, dwBufSize))
                            {
                                Thread.Sleep(2);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
                default:
                    if (dwBufSize > 0 && m_lPort != -1)
                    {
                        //Input the other data
                        for (int i = 0; i < 999; i++)
                        {
                            if (!PlayM4_InputData(m_lPort, pBuffer, dwBufSize))
                            {
                                Thread.Sleep(2);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Decode callback function
        /// </summary>
        private void DecCallbackFUN(int nPort, IntPtr pBuf, int nSize, ref FRAME_INFO pFrameInfo, int nReserved1, int nReserved2)
        {
            //Write the video input after pBuf decoding to the file (the YUV data volume after decoding is huge, especially the high-definition stream, it is not recommended to process it in the callback function)）
            if (pFrameInfo.nType == 3) //#define T_YV12	3
            {
                if (RealDataReceived != null)
                {
                    byte[] byteBuf = new byte[nSize];
                    Marshal.Copy(pBuf, byteBuf, 0, nSize);
                    RealDataReceived.Invoke(this, byteBuf);
                }
            }
        }

        //Stream type
        public const int STREAME_REALTIME = 0;
        public const int STREAME_FILE = 1;

        public const int NET_DVR_SYSHEAD = 1;
        public const int NET_DVR_STREAMDATA = 2;
        public const int NET_DVR_AUDIOSTREAMDATA = 3;
        public const int NET_DVR_STD_VIDEODATA = 4;
        public const int NET_DVR_STD_AUDIODATA = 5;

        // add by gb 080131 version 4.9.0.1
        [DllImport(HikApi.PlayCtrl)]
        public static extern bool PlayM4_GetPort(ref int nPort);

        [DllImport(HikApi.PlayCtrl)]
        public static extern bool PlayM4_SetStreamOpenMode(int nPort, uint nMode);

        [DllImport(HikApi.PlayCtrl)]
        public static extern bool PlayM4_OpenStream(int nPort, IntPtr pFileHeadBuf, uint nSize, uint nBufPoolSize);

        [DllImport(HikApi.PlayCtrl)]
        public static extern bool PlayM4_SetDisplayBuf(int nPort, uint nNum);

        [DllImport(HikApi.PlayCtrl)]
        public static extern bool PlayM4_SetOverlayMode(int nPort, int bOverlay, uint colorKey);

        [DllImport(HikApi.PlayCtrl)]
        public static extern bool PlayM4_SetDecCallBackEx(int nPort, DECCBFUN DecCBFun, IntPtr pDest, int nDestSize);

        [DllImport(HikApi.PlayCtrl)]
        public static extern bool PlayM4_Play(int nPort, IntPtr hWnd);

        [DllImport(HikApi.PlayCtrl)]
        public static extern bool PlayM4_InputData(int nPort, IntPtr pBuf, uint nSize);

        public delegate void DECCBFUN(int nPort, IntPtr pBuf, int nSize, ref FRAME_INFO pFrameInfo, int nReserved1, int nReserved2);

        [DllImport(HikApi.PlayCtrl)]
        public static extern bool PlayM4_SetDecCallBack(int nPort, DECCBFUN DecCBFun);

        [DllImport(HikApi.HCNetSDK)]
        public static extern bool NET_DVR_MakeKeyFrame(int lUserID, int lChannel);

        [DllImport(HikApi.HCNetSDK)]
        public static extern bool NET_DVR_SaveRealData(int lRealHandle, string sFileName);

        [DllImport(HikApi.HCNetSDK)]
        public static extern bool NET_DVR_StopSaveRealData(int lRealHandle);

        [DllImport(HikApi.HCNetSDK)]
        public static extern int NET_DVR_RealPlay_V40(int iUserID, ref NET_DVR_PREVIEWINFO lpPreviewInfo, REALDATACALLBACK fRealDataCallBack_V30, IntPtr pUser);

        public delegate void REALDATACALLBACK(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr pUser);

        [DllImport(HikApi.HCNetSDK)]
        public static extern bool NET_DVR_StopRealPlay(int iRealHandle);
    }
}
