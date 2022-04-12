namespace Hik.Api.Data
{
    public class DeviceConfig
    {
        public int AnalogChannel { get; internal set; }
        public int IPChannel { get; internal set; }
        public int ZeroChannel { get; internal set; }
        public int NetworkPort { get; internal set; }
        public int AlarmInPort { get; internal set; }
        public int AlarmOutPort { get; internal set; }
        public string Serial { get; internal set; }
        public string Version { get; internal set; }
        public string TypeName { get; internal set; }
        public string Name { get; internal set; }
    }
}
