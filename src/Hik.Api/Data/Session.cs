using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Hik.Api.Data
{
    /// <summary>
    /// Session
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Session
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="channelNumber">The channel number.</param>
        /// <param name="ipChannels">The ip channels.</param>
        public Session(int userId, byte channelNumber, IReadOnlyCollection<IpChannel> ipChannels)
        {
            UserId = userId;
            Device = new DeviceInfo(channelNumber, ipChannels);
        }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public int UserId { get; }

        /// <summary>
        /// Gets the device.
        /// </summary>
        /// <value>
        /// The device.
        /// </value>
        public DeviceInfo Device { get; }
    }
}
