using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Lab.Data.Models;
using Newtonsoft.Json;

namespace Lab.Common.Infra
{
    public class ErrorMgrDb: IErrorMgr
    {
        private readonly RequestDTO _request;
        private readonly ErrorItemsRepo _repo;

        public ErrorMgrDb(RequestDTO request)
        {
            _request = request;
            _repo = new ErrorItemsRepo();
        }

        public async Task<IEnumerable<ErrorPoco>> GetErrorItems(int count=100)
        {
            var res = await _repo.GetErrorItems(count);
            
            return res.Select(e => new ErrorPoco
            {
                ErrorDate = e.ErrorDate,
                ErrorMessage = e.ErrorMessage,
                ErrorSource = e.ErrorSource,
                Id = e.Id,
                InnerExceptionMessage = e.InnerExceptionMessage,
                IPAddress = e.IPAddress,
                PostData = e.PostData,
                QSData = e.QSData,
                Referrer = e.Referrer,
                StackTrace = e.StackTrace,
                Status = e.Status,
                UserAgent = e.UserAgent,
                UserComment = e.UserComment
            }).ToList();
        }

        public async Task<ErrorPoco> ReadError(string id, bool track=true)
        {
            return ConvertDbToPoco(await _repo.GetErrorItem(id));
        }

        private ErrorLog ConvertPocoToDb(ErrorPoco item)
        {
            return JsonConvert.DeserializeObject<ErrorLog>(JsonConvert.SerializeObject(item));
        }

        private ErrorPoco ConvertDbToPoco(ErrorLog item)
        {
            return JsonConvert.DeserializeObject<ErrorPoco>(JsonConvert.SerializeObject(item));
        }

        public async Task<bool> SaveError(ErrorPoco eo)
        {
            try
            {
                var dbItem = ConvertPocoToDb(eo);

                if (eo.Id == null)
                {
                    await _repo.InsertErrorItem(dbItem);
                }
                else
                {
                    await _repo.UpdateErrorItem(dbItem);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error saving SiteError object to DB: " + ex.Message, EventLogEntryType.Error);
                return false;
            }
        }

        public async Task<ErrResponsePoco> InsertErrorItem(ErrorLog item)
        {
            item = await _repo.InsertErrorItem(item);

            return new ErrResponsePoco
            {
                DbErrorId = item.Id
            };
        }

        public async Task<ErrResponsePoco> InsertError(Exception err, string message = "", string userComment="")
        {
            try
            {
                var baseException = err.GetBaseException();
                var item = new ErrorLog
                {
                    InnerExceptionMessage = ((err.InnerException != null) ? baseException.ToString() : ""),
                    InnerExceptionSource = ((err.InnerException != null) ? baseException.Source : ""),
                    Status = ErrorItemStatus.New.ToString(),
                    StackTrace = err.StackTrace ?? "N/A",
                    ErrorDate = DateTime.UtcNow,
                    ErrorSource = err.Source ?? "N/A",
                    ErrorMessage = err.Message,
                    UserComment = userComment,
                    AdditionalMessage = message
                };

                if (_request == null) return await InsertErrorItem(item);

                item.UserAgent = _request.UserAgent;
                item.IPAddress = _request.IPAddress;
                item.UserName = _request.UserName;
                item.Referrer = (_request.UrlReferrer==null) ? "N/A" : _request.UrlReferrer.ToString();
                item.PostData = WebUtility.HtmlEncode(_request.Form.ToString());
                item.QSData = WebUtility.HtmlEncode((_request.Url == null) ? "N/A" : _request.Url.OriginalString);
                return await InsertErrorItem(item);
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog(err.Message, EventLogEntryType.Error, ex);
                throw ex;
            }
        }
    }
}
