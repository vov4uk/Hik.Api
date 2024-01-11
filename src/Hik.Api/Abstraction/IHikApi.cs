using System;

namespace Hik.Api.Abstraction
{
    /// <summary>
    /// Hikvision SDK Wrapper
    /// </summary>
    public interface IHikApi
    {
        /// <summary>
        /// Occurs when [disconnected].
        /// </summary>
        event EventHandler Disconnected;
        /// <summary>
        /// Gets the video service.
        /// </summary>
        /// <value>
        /// The video service.
        /// </value>
        IVideoService VideoService { get; }

        /// <summary>
        /// Gets the photo service.
        /// </summary>
        /// <value>
        /// The photo service.
        /// </value>
        IPhotoService PhotoService { get; }

        /// <summary>
        /// Gets the playback service.
        /// </summary>
        /// <value>
        /// The playback service.
        /// </value>
        IPlaybackService PlaybackService { get; }

        /// <summary>
        /// Gets the config service.
        /// </summary>
        /// <value>
        /// The config service.
        /// </value>
        IConfigService ConfigService { get; }

        /// <summary>Logouts the user.</summary>
        void Logout();
    }
}
