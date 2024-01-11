using System;

namespace Hik.Api.Abstraction
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPlaybackService
    {
        /// <summary>
        /// Starts the play back.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="playbackWindowHandler">The playback window handler.</param>
        /// <returns></returns>
        int StartPlayBack(int channel, IntPtr? playbackWindowHandler = null);
        /// <summary>
        /// Stops the play back.
        /// </summary>
        /// <param name="playbackId">The playback identifier.</param>
        /// <returns></returns>
        bool StopPlayBack(int playbackId);
        /// <summary>
        /// Starts the recording.
        /// </summary>
        /// <param name="playbackId">The playback identifier.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        bool StartRecording(int playbackId, string filePath, int channel);
        /// <summary>
        /// Stops the recording.
        /// </summary>
        /// <param name="playbackId">The playback identifier.</param>
        /// <returns></returns>
        bool StopRecording(int playbackId);
    }
}
