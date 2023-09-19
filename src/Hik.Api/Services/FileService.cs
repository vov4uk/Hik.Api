using Hik.Api.Abstraction;
using Hik.Api.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hik.Api.Services
{
    /// <summary>
    /// Base class to download files
    /// </summary>
    public abstract class FileService
    {
        /// <summary>
        /// Finds the files asynchronous.
        /// </summary>
        /// <param name="periodStart">The period start.</param>
        /// <param name="periodEnd">The period end.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public virtual Task<IReadOnlyCollection<HikRemoteFile>> FindFilesAsync(DateTime periodStart, DateTime periodEnd, Session session)
        {
            return FindFiles(periodStart, periodEnd, session, session.Device.DefaultIpChannel);
        }

        /// <summary>
        /// Get files list for specific channel
        /// </summary>
        /// <param name="periodStart">The period start.</param>
        /// <param name="periodEnd">The period end.</param>
        /// <param name="session">The session.</param>
        /// <param name="ipChannel">The ip channel.</param>
        /// <returns></returns>
        public virtual Task<IReadOnlyCollection<HikRemoteFile>> FindFilesAsync(DateTime periodStart, DateTime periodEnd, Session session, int ipChannel)
        {
            return FindFiles(periodStart, periodEnd, session, ipChannel);
        }

        /// <summary>
        /// Starts the find.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="periodStart">The period start.</param>
        /// <param name="periodEnd">The period end.</param>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        protected abstract int StartFind(int userId, DateTime periodStart, DateTime periodEnd, int channel);

        /// <summary>
        /// Stops the find.
        /// </summary>
        /// <param name="findId">The find identifier.</param>
        /// <returns>Success</returns>
        protected abstract bool StopFind(int findId);

        /// <summary>
        /// Finds the next.
        /// </summary>
        /// <param name="findId">The find identifier.</param>
        /// <param name="source">The source.</param>
        /// <returns>Success</returns>
        internal abstract int FindNext(int findId, ref ISourceFile source);


        /// <summary>
        /// Gets the find results.
        /// </summary>
        /// <param name="findId">The find identifier.</param>
        /// <returns></returns>
        protected async Task<IReadOnlyCollection<HikRemoteFile>> GetFindResults(int findId)
        {
            var results = new List<HikRemoteFile>();
            ISourceFile sourceFile = default(ISourceFile);
            while (true)
            {
                int findStatus = FindNext(findId, ref sourceFile);

                if (findStatus == HikConst.NET_DVR_ISFINDING)
                {
                    await Task.Delay(500);
                }
                else if (findStatus == HikConst.NET_DVR_FILE_SUCCESS)
                {
                    results.Add(sourceFile.ToRemoteFile());
                }
                else
                {
                    break;
                }
            }

            return results;
        }

        private async Task<IReadOnlyCollection<HikRemoteFile>> FindFiles(DateTime periodStart, DateTime periodEnd, Session session, int ipChannel)
        {
            int findId = this.StartFind(session.UserId, periodStart, periodEnd, ipChannel);

            IEnumerable<HikRemoteFile> results = await this.GetFindResults(findId);

            this.StopFind(findId);
            return results.ToList();
        }
    }
}
