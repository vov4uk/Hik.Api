using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Hik.Api.Struct
{
    [ExcludeFromCodeCoverage]
    [StructLayout(LayoutKind.Sequential)]
    internal struct NET_DVR_TIME
    {
        public uint dwYear;
        public uint dwMonth;
        public uint dwDay;
        public uint dwHour;
        public uint dwMinute;
        public uint dwSecond;

        public NET_DVR_TIME(DateTime dateTime)
        {
            this.dwYear = (uint)dateTime.Year;
            this.dwMonth = (uint)dateTime.Month;
            this.dwDay = (uint)dateTime.Day;
            this.dwHour = (uint)dateTime.Hour;
            this.dwMinute = (uint)dateTime.Minute;
            this.dwSecond = (uint)dateTime.Second;
        }

        public override string ToString()
        {
            return $"{this.dwYear:0000}-{this.dwMonth:00}-{this.dwDay:00}_{this.dwHour:00}:{this.dwMinute:00}:{this.dwSecond:00}";
        }

        public DateTime ToDateTime()
        {
            return new DateTime((int)this.dwYear, (int)this.dwMonth, (int)this.dwDay, (int)this.dwHour, (int)this.dwMinute, (int)this.dwSecond);
        }
    }
}
