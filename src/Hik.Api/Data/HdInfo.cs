using System;
using System.Collections.Generic;
using System.Text;
using Hik.Api.Struct.Config;

namespace Hik.Api.Data
{
    /// <summary>
    ///  Hard drive information
    /// </summary>
    public class HdInfo
    {
        /// <summary>Initializes a new instance of the <see cref="HdInfo" /> class.</summary>
        public HdInfo() { }
        internal HdInfo(NET_DVR_SINGLE_HD hd)
        {
            Capacity = hd.dwCapacity;
            FreeSpace = hd.dwFreeSpace;
            HdStatus = hd.dwHdStatus;
            HDAttr = hd.byHDAttr;
            HDType = hd.byHDType;
            Recycling = hd.byRecycling;
            PictureCapacity = hd.dwPictureCapacity;
            FreePictureSpace = hd.dwFreePictureSpace;
        }

        /// <summary>Gets a value indicating whether this instance is error status.</summary>
        /// <value>
        ///   <c>true</c> if this instance is error status; otherwise, <c>false</c>.</value>
        public bool IsErrorStatus => HdStatus == 2;
        /// <summary>Gets or sets the capacity.</summary>
        /// <value>The capacity.</value>
        public uint Capacity { get; set; }
        /// <summary>Gets or sets the free space.</summary>
        /// <value>The free space.</value>
        public uint FreeSpace { get; set; }
        /// <summary>Gets or sets the hd status.</summary>
        /// <value>The hd status.</value>
        public uint HdStatus { get; set; }
        /// <summary>Gets or sets the hd attribute.</summary>
        /// <value>The hd attribute.</value>
        public byte HDAttr { get; set; }
        /// <summary>Gets or sets the type of the hd.</summary>
        /// <value>The type of the hd.</value>
        public byte HDType { get; set; }
        /// <summary>Gets or sets the recycling.</summary>
        /// <value>The recycling.</value>
        public byte Recycling { get; set; }
        /// <summary>Gets or sets the picture capacity.</summary>
        /// <value>The picture capacity.</value>
        public uint PictureCapacity { get; set; }
        /// <summary>Gets or sets the free picture space.</summary>
        /// <value>The free picture space.</value>
        public uint FreePictureSpace { get; set; }

        /// <summary>Converts to string.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine(GetRow(nameof(Capacity), ToGB(Capacity)));
            sb.AppendLine(GetRow(nameof(FreeSpace), ToGB(FreeSpace)));
            sb.AppendLine(GetRow(nameof(PictureCapacity), ToGB(PictureCapacity)));
            sb.AppendLine(GetRow(nameof(FreePictureSpace), ToGB(FreePictureSpace)));
            sb.AppendLine(GetRow(nameof(HdStatus), HdStatuses.TryGetValue(HdStatus, out var status) ? status : "unknown"));
            sb.AppendLine(GetRow(nameof(HDAttr), HdAttributes.TryGetValue(HDAttr, out var atr) ? atr : "unknown"));
            sb.AppendLine(GetRow(nameof(HDType), HdTypes.TryGetValue(HDType, out var hdType) ? hdType : "unknown"));
            sb.AppendLine(GetRow(nameof(Recycling), Convert.ToString(Recycling)));

            return sb.ToString();
        }

        private static readonly Dictionary<uint, string> HdStatuses = new Dictionary<uint, string>
        {
            {0, "normal"},
            {1, "unformatted"},
            {2, "error"},
            {3, "S.M.A.R.T state"},
            {4, "not match"},
            {5, "sleeping"},
            {6, "unconnected(network disk)"},
            {7, "virtual disk is normal and supports expansion"},
            {10, "hard disk is being restored"},
            {11, "hard disk is being formatted"},
            {12, "hard disk is waiting formatted"},
            {13, "the hard disk has been uninstalled"},
            {14, "local hard disk does not exist"},
            {15, "it is deleting the network disk"},
            {16, "locked"}
        };
        
        private static readonly Dictionary<uint, string> HdAttributes = new Dictionary<uint, string>
        {
            {0, "default"},
            {1, "redundancy (back up important data)"},
            {2, "read only"},
            {3, "Archiving"},
            {4, "Cannot be read/read"}
        };   
        
        private static readonly Dictionary<uint, string> HdTypes = new Dictionary<uint, string>
        {
            {0, "local disk"},
            {1, "eSATA disk"},
            {2, "NFS disk"},
            {3, "iSCSI disk"},
            {4, "RAID virtual disk"},
            {5, "SD card"},
            {6, "miniSAS"}
        };

        private string ToGB(uint mb)
        {
            return $"{mb / 1024.0:0.00} GB ({mb} Mb)";
        }

        private string GetRow(string field, string value)
        {
            return $"{field,-24}: {value}";
        }
    }
}