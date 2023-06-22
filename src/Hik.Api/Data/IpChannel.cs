namespace Hik.Api.Data
{
    public class IpChannel
    {
        public IpChannel(int iChanNo, byte byOnline, byte byIPI)
        {
            ChannelNumber = iChanNo;
            IsOnline = byOnline != 0;
            IsEmpty = byIPI == 0;
        }

        public int ChannelNumber { get; }
        public bool IsOnline { get; }
        public bool IsEmpty { get; }

        public string Name { get; set; }
    }
}