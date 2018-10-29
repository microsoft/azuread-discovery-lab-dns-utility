using Lab.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab.Common.Infra
{
    public interface IErrorItemsRepo
    {
        Task<ErrorLog> InsertErrorItem(ErrorLog item);

        Task<ErrorLog> GetErrorItem(string id);

        Task<IEnumerable<ErrorLog>> GetErrorItems(int count = 100);

        Task<IEnumerable<ErrorLog>> FindErrorItems(string search);

        Task<IEnumerable<ErrorLog>> GetMatchingErrorItems(string id);

        Task<int> DeleteMatchingErrorItems(string id);

        Task<int> DeleteErrorItem(string id);

        Task<ErrorLog> UpdateErrorItem(ErrorLog item);
    }

    public enum ErrorItemStatus
    {
        New,
        Assigned,
        Resolved,
        WillNotFix,
        Closed
    }
}
