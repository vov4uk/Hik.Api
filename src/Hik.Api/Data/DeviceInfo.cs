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
        /// <param name="analogChannels">The analog channels.</param>
        public DeviceInfo(byte defaultIpChannel, IReadOnlyCollection<IpChannel> ipChannels, IReadOnlyCollection<IpChannel> analogChannels)
        {
            DefaultIpChannel = defaultIpChannel;
            IpChannels = ipChannels;
            AnalogChannels = analogChannels;
        }

        /// <summary>Gets the default ip channel.</summary>
        /// <value>The default ip channel.</value>
        public byte DefaultIpChannel { get; }

        /// <summary>Gets the ip channels.</summary>
        /// <value>The ip channels.</value>
        public IReadOnlyCollection<IpChannel> IpChannels { get; }
        /// <summary>Gets the analog channels.</summary>
        /// <value>The analog channels.</value>
        public IReadOnlyCollection<IpChannel> AnalogChannels { get; }
    }
}