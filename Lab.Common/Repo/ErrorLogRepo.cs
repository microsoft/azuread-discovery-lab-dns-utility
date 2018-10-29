using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocDBLib;
using System.Linq.Expressions;
using Lab.Data.Models;

namespace Lab.Common.Repo
{
    public class ErrorLogRepo : BaseRepo<ErrorLog>, IRepo<ErrorLog>
    {
        public async Task<IEnumerable<ErrorLog>> GetMostRecent100()
        {
            var res = await DocDBRepo.DB<ErrorLog>.GetItemsAsync(100);
            return res.OrderByDescending(l => l.ErrorDate);
        }

        public async Task<IEnumerable<ErrorLog>> SearchLogs(string search)
        {
            var res = await DocDBRepo.DB<ErrorLog>.GetItemsAsync(e => e.ErrorMessage.Contains(search) || e.ErrorSource.Contains(search));
            return res.OrderByDescending(l => l.ErrorDate);
        }

        public async Task<IEnumerable<ErrorLog>> GetMatchingErrorItems(string id)
        {
            var source = await GetAsync(id);
            var res = await DocDBRepo.DB<ErrorLog>.GetItemsAsync(e => e.ErrorSource == source.ErrorSource && e.ErrorMessage == source.ErrorMessage);
            return res.OrderByDescending(e => e.ErrorDate).ToList();
        }

        public async Task<int> DeleteMatchingErrorItems(string id)
        {
            var res = 0;
            var list = await GetMatchingErrorItems(id);
            foreach(var item in list)
            {
                await Delete(item.Id);
                res++;
            }
            return res;
        }

        public async Task<IEnumerable<ErrorLog>> GetErrorItemsByStatus(string status)
        {
            return await DocDBRepo.DB<ErrorLog>.GetItemsAsync(e => e.Status == status);
        }
    }
}
