using System.Diagnostics;

namespace Hik.Api.Data
{
    /// <summary>
    /// IpChannel
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class IpChannel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IpChannel"/> class.
        /// </summary>
        /// <param name="iChanNo">The i chan no.</param>
        /// <param name="byOnline">if set to <c>true</c> [by online].</param>
        /// <param name="name">The name.</param>
        public IpChannel(int iChanNo, bool byOnline, string name)
        {
            ChannelNumber = iChanNo;
            IsOnline = byOnline;
            Name = name;
        }

        /// <summary>
        /// Gets the channel number.
        /// </summary>
        /// <value>
        /// The channel number.
        /// </value>
        public int ChannelNumber { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is online.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is online; otherwise, <c>false</c>.
        /// </value>
        public bool IsOnline { get; }
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }
    }
}