using Hik.Api.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hik.Api.Abstraction
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Finds the files asynchronous.
        /// </summary>
        /// <param name="periodStart">The period start.</param>
        /// <param name="periodEnd">The period end.</param>
        /// <returns></returns>
        Task<IReadOnlyCollection<HikRemoteFile>> FindFilesAsync(DateTime periodStart, DateTime periodEnd);

        /// <summary>
        /// Finds the files asynchronous.
        /// </summary>
        /// <param name="periodStart">The period start.</param>
        /// <param name="periodEnd">The period end.</param>
        /// <param name="ipChannel">The ip channel.</param>
        /// <returns></returns>
        Task<IReadOnlyCollection<HikRemoteFile>> FindFilesAsync(DateTime periodStart, DateTime periodEnd, int ipChannel);
    }
}
