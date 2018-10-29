using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lab.Common.Repo;
using Lab.Data.Models;

namespace Lab.Common.Infra
{
    public class ErrorItemsRepo: IErrorItemsRepo
    {
        ErrorLogRepo _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorItemsRepo" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ErrorItemsRepo()
        {
            _log = new ErrorLogRepo();
        }
        public async Task<ErrorLog> InsertErrorItem(ErrorLog item)
        {
            var res = await _log.Upsert(item);
            return res;
        }

        public async Task<ErrorLog> GetErrorItem(string id)
        {
            return await _log.GetAsync(id);
        }

        public async Task<IEnumerable<ErrorLog>> GetErrorItems(int count = 100)
        {
            return await _log.GetMostRecent100();
        }

        public async Task<IEnumerable<ErrorLog>> FindErrorItems(string search)
        {
            return await _log.SearchLogs(search);
        }

        public async Task<IEnumerable<ErrorLog>> GetMatchingErrorItems(string id)
        {
            return await _log.GetMatchingErrorItems(id);
        }

        public async Task<int> DeleteMatchingErrorItems(string id)
        {
            return await _log.DeleteMatchingErrorItems(id);
        }

        public async Task<int> DeleteErrorItem(string id)
        {
            return await _log.Delete(id);
        }

        public async Task<ErrorLog> UpdateErrorItem(ErrorLog item)
        {
            return await _log.Upsert(item);
        }
    }
}
