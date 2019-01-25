using AzureADLabDNSControl.Infra;
using Lab.Common;
using Lab.Common.Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureADLabDNSControl.Controllers.api
{
    public class Err
    {
        public string ErrorId { get; set; }
        public string Status { get; set; }
        public DateTime DeleteBefore { get; set; }
    }

    [AdminAuthorize(Roles = CustomRoles.LabAdmin)]
    public class ErrorLogController : ApiController
    {
        private ErrorItemBL _err;

        public ErrorLogController()
        {
            _err = new ErrorItemBL();
        }

        public async Task<ErrorPoco> GetErrorItem(string id)
        {
            return await _err.GetErrorItem(id);
        }

        public async Task<IEnumerable<ErrorPoco>> GetErrorItems(int count)
        {
            var res =  await _err.GetErrorItems(count);
            return res;
        }

        public async Task<IEnumerable<ErrorPoco>> FindErrorItems(string search)
        {
            return await _err.FindErrorItems(search);
        }

        public async Task<IEnumerable<ErrorPoco>> GetMatchingErrorItems(string id)
        {
            return await _err.GetMatchingErrorItems(id);
        }

        [HttpDelete]
        public async Task<ErrorItemsPoco> DeleteMatchingErrorItems(Err err)
        {
            var res = new ErrorItemsPoco();
            var count = await _err.DeleteMatchingErrorItems(err.ErrorId);
            res.RecordCount = count;
            res.ErrorItems = await _err.GetErrorItems();
            return res;
        }

        [HttpDelete]
        public async Task<ErrorItemsPoco> DeleteErrorItem(Err err)
        {
            var res = new ErrorItemsPoco
            {
                RecordCount = await _err.DeleteErrorItem(err.ErrorId),
                ErrorItems = await _err.GetErrorItems()
            };
            return res;
        }
    }
}
