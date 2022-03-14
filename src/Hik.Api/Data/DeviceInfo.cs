using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Hik.Api.Data
{
    [ExcludeFromCodeCoverage]
    public class DeviceInfo
    {
        public DeviceInfo(byte defaultIpChannel, IReadOnlyCollection<IpChannel> ipChannels)
        {
            DefaultIpChannel = defaultIpChannel;
            IpChannels = ipChannels;
        }

        public byte DefaultIpChannel { get; }

        public IReadOnlyCollection<IpChannel> IpChannels { get; }
    }
}