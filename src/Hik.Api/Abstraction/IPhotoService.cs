using Hik.Api.Data;

namespace Hik.Api.Abstraction
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Hik.Api.Abstraction.IFileService" />
    public interface IPhotoService : IFileService
    {
        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="remoteFileName">Name of the remote file.</param>
        /// <param name="size">The size.</param>
        /// <param name="destinationPath">The destination path.</param>
        void DownloadFile(string remoteFileName, long size, string destinationPath);

        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="photo">The photo.</param>
        /// <param name="destinationPath">The destination path.</param>
        void DownloadFile(HikRemoteFile photo, string destinationPath);
    }
}
