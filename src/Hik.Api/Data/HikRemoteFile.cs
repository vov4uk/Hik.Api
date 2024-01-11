using Hik.Api.Struct.Photo;
using Hik.Api.Struct.Video;
using System;

namespace Hik.Api.Data
{
    /// <summary>
    ///  Remote file from camera
    /// </summary>
    public class HikRemoteFile
    {
        internal HikRemoteFile(NET_DVR_FIND_PICTURE_V50 findData)
        {
            this.Name = findData.sFileName;
            this.Date = findData.struTime.ToDateTime();
            this.Size = findData.dwFileSize;
        }

        internal HikRemoteFile(NET_DVR_FINDDATA_V30 findData)
        {
            this.Name = findData.sFileName;
            this.Date = findData.struStartTime.ToDateTime();
            this.Duration = (int)(findData.struStopTime.ToDateTime() - findData.struStartTime.ToDateTime()).TotalSeconds;
            this.Size = findData.dwFileSize;
        }

        /// <summary>Initializes a new instance of the <see cref="HikRemoteFile" /> class.</summary>
        public HikRemoteFile()
        {
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public long Size { get; set; }
    }
}
