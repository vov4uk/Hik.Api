using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Hik.Api.Data
{
    [ExcludeFromCodeCoverage]
    public class Session
    {
        public Session(int userId, byte channelNumber, IReadOnlyCollection<IpChannel> ipChannels)
        {
            UserId = userId;
            Device = new DeviceInfo(channelNumber, ipChannels);
        }

        public int UserId { get; }

        public DeviceInfo Device { get; }
    }
}
