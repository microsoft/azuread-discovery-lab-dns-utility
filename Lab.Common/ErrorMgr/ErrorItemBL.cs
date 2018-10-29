using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using Lab.Data.Models;

namespace Lab.Common.Infra
{
    public class ErrorItemBL
    {
        private readonly IErrorItemsRepo _repo;

        public ErrorItemBL()
        {
            _repo = new ErrorItemsRepo();
        }

        public async Task<ErrorPoco> GetErrorItem(string id)
        {
            var item = await _repo.GetErrorItem(id);
            return GetErrorItemPoco(item);
        }

        public async Task<IEnumerable<ErrorPoco>> GetErrorItems(int count = 100)
        {
            return (await _repo.GetErrorItems(count)).Select(GetErrorItemPoco);
        }

        public async Task<IEnumerable<ErrorPoco>> FindErrorItems(string search)
        {
            return (await _repo.FindErrorItems(search)).Select(GetErrorItemPoco);
        }

        public async Task<IEnumerable<ErrorPoco>> GetMatchingErrorItems(string id)
        {
            return (await _repo.GetMatchingErrorItems(id)).Select(GetErrorItemPoco);
        }

        public async Task<int> DeleteMatchingErrorItems(string id)
        {
            return await _repo.DeleteMatchingErrorItems(id);
        }

        public async Task<int> DeleteErrorItem(string id)
        {
            return await _repo.DeleteErrorItem(id);
        }

        public async Task<ErrorPoco> UpdateErrorItemUserComment(string id, string comment)
        {
            var item = await _repo.GetErrorItem(id);
            item.UserComment = comment;
            return GetErrorItemPoco(await _repo.UpdateErrorItem(item));
        }

        #region helpers
        private static ErrorPoco GetErrorItemPoco(ErrorLog err)
        {
            return new ErrorPoco
            {
                ErrorDate = err.ErrorDate,
                Id = err.Id,
                ErrorMessage = err.ErrorMessage,
                ErrorSource = err.ErrorSource,
                InnerExceptionMessage = err.InnerExceptionMessage,
                InnerExceptionSource = err.InnerExceptionSource,
                IPAddress = err.IPAddress,
                PostData = err.PostData,
                QSData = err.QSData,
                UserComment = err.UserComment,
                Email = err.Email,
                UserName = err.UserName,
                AdditionalMessage = err.AdditionalMessage,
                Referrer = err.Referrer,
                StackTrace = err.StackTrace,
                Status = err.Status,
                UserAgent = err.UserAgent,
                ValidationErrors = err.ValidationErrors
            };
        }
        #endregion
    }
}
