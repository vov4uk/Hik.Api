using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Hik.Api.Data
{
    /// <summary>
    ///  Device Info
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DeviceInfo
    {
        /// <summary>Initializes a new instance of the <see cref="DeviceInfo" /> class.</summary>
        /// <param name="defaultIpChannel">The default ip channel.</param>
        /// <param name="ipChannels">The ip channels.</param>
        public DeviceInfo(byte defaultIpChannel, IReadOnlyCollection<IpChannel> ipChannels)
        {
            DefaultIpChannel = defaultIpChannel;
            IpChannels = ipChannels;
        }

        /// <summary>Gets the default ip channel.</summary>
        /// <value>The default ip channel.</value>
        public byte DefaultIpChannel { get; }

        /// <summary>Gets the ip channels.</summary>
        /// <value>The ip channels.</value>
        public IReadOnlyCollection<IpChannel> IpChannels { get; }
    }
}