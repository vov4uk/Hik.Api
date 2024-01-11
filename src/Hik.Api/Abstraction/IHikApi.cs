using Hik.Api.Services;

namespace Hik.Api.Abstraction
{
    /// <summary>
    /// Hikvision SDK Wrapper
    /// </summary>
    public interface IHikApi
    {
        /// <summary>
        /// Gets the video service.
        /// </summary>
        /// <value>
        /// The video service.
        /// </value>
        VideoService VideoService { get; }

        /// <summary>
        /// Gets the photo service.
        /// </summary>
        /// <value>
        /// The photo service.
        /// </value>
        PhotoService PhotoService { get; }

        /// <summary>
        /// Gets the playback service.
        /// </summary>
        /// <value>
        /// The playback service.
        /// </value>
        PlaybackService PlaybackService { get; }

        /// <summary>
        /// Gets the config service.
        /// </summary>
        /// <value>
        /// The config service.
        /// </value>
        ConfigService ConfigService { get; }

        /// <summary>Logouts the user.</summary>
        void Logout();
    }
}
