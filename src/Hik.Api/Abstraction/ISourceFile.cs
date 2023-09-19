using Hik.Api.Data;

namespace Hik.Api.Abstraction
{
    internal interface ISourceFile
    {
        /// <summary>
        /// Converts to remotefile.
        /// </summary>
        /// <returns></returns>
        HikRemoteFile ToRemoteFile();
    }
}
