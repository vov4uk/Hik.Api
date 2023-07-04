using Hik.Api.Data;

namespace Hik.Api.Helpers
{
    /// <summary>
    /// Hik Remote File Extentions
    /// </summary>
    public static class HikRemoteFileExtentions
    {
        private const string StartDateTimePrintFormat = "yyyy.MM.dd HH:mm:ss";
        private const string DateFormat = "yyyyMMdd_HHmmss";
        private const string TimeFormat = "HHmmss";
        private static readonly string[] Suffix = { "B", "KB", "MB", "GB", "TB" };

        /// <summary>
        /// Converts to user friendly string. For video files
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public static string ToVideoUserFriendlyString(this HikRemoteFile file)
        {
            return $"{file.Name} | {file.Date.ToString(StartDateTimePrintFormat)} - {file.Duration} | {FormatBytes(file.Size)}";
        }

        /// <summary>
        /// Converts to user friendly file name. For photo files.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public static string ToPhotoFileNameString(this HikRemoteFile file)
        {
            return $"{file.Date.ToString(DateFormat)}.jpg";
        }

        /// <summary>
        /// Gets Relative path in format YYYY-MM\\DD\\HH
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public static string ToDirectoryNameString(this HikRemoteFile file)
        {
            return $"{file.Date.Year:0000}-{file.Date.Month:00}\\{file.Date.Day:00}\\{file.Date.Hour:00}";
        }

        /// <summary>
        /// Converts to user friendly file name. For video files.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public static string ToVideoFileNameString(this HikRemoteFile file)
        {
            return $"{file.Date.ToString(DateFormat)}_{file.Date.AddSeconds(file.Duration).ToString(TimeFormat)}.mp4";
        }

        private static string FormatBytes(this long bytes)
        {
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return $"{dblSByte,6:0.00} {Suffix[i]}";
        }
    }
}
