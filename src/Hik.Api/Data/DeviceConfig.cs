namespace Hik.Api.Data
{
    /// <summary>
    /// Device Config
    /// </summary>
    public class DeviceConfig
    {
        /// <summary>
        /// Gets the analog channel.
        /// </summary>
        /// <value>
        /// The analog channel.
        /// </value>
        public int AnalogChannel { get; internal set; }
        /// <summary>
        /// Gets the ip channel.
        /// </summary>
        /// <value>
        /// The ip channel.
        /// </value>
        public int IPChannel { get; internal set; }
        /// <summary>
        /// Gets the zero channel.
        /// </summary>
        /// <value>
        /// The zero channel.
        /// </value>
        public int ZeroChannel { get; internal set; }
        /// <summary>
        /// Gets the network port.
        /// </summary>
        /// <value>
        /// The network port.
        /// </value>
        public int NetworkPort { get; internal set; }
        /// <summary>
        /// Gets the alarm in port.
        /// </summary>
        /// <value>
        /// The alarm in port.
        /// </value>
        public int AlarmInPort { get; internal set; }
        /// <summary>
        /// Gets the alarm out port.
        /// </summary>
        /// <value>
        /// The alarm out port.
        /// </value>
        public int AlarmOutPort { get; internal set; }
        /// <summary>
        /// Gets the serial.
        /// </summary>
        /// <value>
        /// The serial.
        /// </value>
        public string Serial { get; internal set; }
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; internal set; }
        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>
        /// The name of the type.
        /// </value>
        public string TypeName { get; internal set; }
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; internal set; }
    }
}
