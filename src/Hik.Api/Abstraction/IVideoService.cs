namespace Hik.Api.Abstraction
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Hik.Api.Abstraction.IFileService" />
    public interface IVideoService : IFileService
    {
        /// <summary>
        /// Starts the download file.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <returns></returns>
        int StartDownloadFile(string sourceFile, string destinationPath);

        /// <summary>
        /// Stops the download file.
        /// </summary>
        /// <param name="fileHandle">The file handle.</param>
        void StopDownloadFile(int fileHandle);

        /// <summary>
        /// Gets the download position.
        /// </summary>
        /// <param name="fileHandle">The file handle.</param>
        /// <returns></returns>
        int GetDownloadPosition(int fileHandle);
    }
}
