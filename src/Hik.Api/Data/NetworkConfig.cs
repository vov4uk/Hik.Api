namespace Hik.Api.Data
{
    public class NetworkConfig
    {
        public string IPAddress { get; internal set; }
        public string GateWay { get; internal set; }
        public string SubMask { get; internal set; }
        public string Dns { get; internal set; }
        public string HostIP { get; internal set; }
        public string PPPoEName { get; internal set; }
        public string PPPoEPassword { get; internal set; }
        public int AlarmHostIpPort { get; internal set; }
        public int HttpPort { get; internal set; }
        public int DVRPort { get; internal set; }
        public bool DHCP { get; internal set; }
        public bool PPPoE { get; internal set; }
    }
}