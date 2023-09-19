namespace Hik.Api.Data
{
    /// <summary>
    /// Network config
    /// </summary>
    public class NetworkConfig
    {
        /// <summary>
        /// Gets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        public string IPAddress { get; internal set; }
        /// <summary>
        /// Gets the gate way.
        /// </summary>
        /// <value>
        /// The gate way.
        /// </value>
        public string GateWay { get; internal set; }
        /// <summary>
        /// Gets the sub mask.
        /// </summary>
        /// <value>
        /// The sub mask.
        /// </value>
        public string SubMask { get; internal set; }
        /// <summary>
        /// Gets the DNS.
        /// </summary>
        /// <value>
        /// The DNS.
        /// </value>
        public string Dns { get; internal set; }
        /// <summary>
        /// Gets the host ip.
        /// </summary>
        /// <value>
        /// The host ip.
        /// </value>
        public string HostIP { get; internal set; }
        /// <summary>
        /// Gets the name of the pp po e.
        /// </summary>
        /// <value>
        /// The name of the pp po e.
        /// </value>
        public string PPPoEName { get; internal set; }
        /// <summary>
        /// Gets the pp po e password.
        /// </summary>
        /// <value>
        /// The pp po e password.
        /// </value>
        public string PPPoEPassword { get; internal set; }
        /// <summary>
        /// Gets the alarm host ip port.
        /// </summary>
        /// <value>
        /// The alarm host ip port.
        /// </value>
        public int AlarmHostIpPort { get; internal set; }
        /// <summary>
        /// Gets the HTTP port.
        /// </summary>
        /// <value>
        /// The HTTP port.
        /// </value>
        public int HttpPort { get; internal set; }
        /// <summary>
        /// Gets the DVR port.
        /// </summary>
        /// <value>
        /// The DVR port.
        /// </value>
        public int DVRPort { get; internal set; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="NetworkConfig"/> is DHCP.
        /// </summary>
        /// <value>
        ///   <c>true</c> if DHCP; otherwise, <c>false</c>.
        /// </value>
        public bool DHCP { get; internal set; }
        /// <summary>
        /// Gets a value indicating whether [pp po e].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [pp po e]; otherwise, <c>false</c>.
        /// </value>
        public bool PPPoE { get; internal set; }
    }
}