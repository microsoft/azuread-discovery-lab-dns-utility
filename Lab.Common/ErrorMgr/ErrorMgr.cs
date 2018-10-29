using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Lab.Common.Infra
{
    public class ErrorMgr: IErrorMgr
    {
        private readonly IErrorMgr _mgr;
        public ErrorMgr(RequestDTO request)
        {
            _mgr = (IErrorMgr)new ErrorMgrDb(request);
        }

        public async Task<IEnumerable<ErrorPoco>> GetErrorItems(int count=100)
        {
            return await _mgr.GetErrorItems(count);
        }

        public async Task<ErrorPoco> ReadError(string id, bool track=true)
        {
            return await _mgr.ReadError(id, track);
        }
        public async Task<bool> SaveError(ErrorPoco eo)
        {
            return await _mgr.SaveError(eo);
        }
        public async Task<ErrResponsePoco> InsertError(Exception err, string message = "", string userComment="")
        {
            return await _mgr.InsertError(err, message, userComment);
        }

        /// <summary>
        /// Writes an error entry to the Application log, Application Source. This is a fallback error writing mechanism.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errorType">Type of error.</param>
        public void WriteToAppLog(string message, EventLogEntryType errorType)
        {
            Logging.WriteToAppLog(message, errorType);
        }

    }
}
