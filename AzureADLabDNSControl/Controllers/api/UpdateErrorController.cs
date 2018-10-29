using System;
using System.Text;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Lab.Common;
using Lab.Common.Infra;

namespace AzureADLabDNSControl.Controllers.api
{
    public class ErrUpdatePoco
    {
        public string Comment { get; set; }
        public string Error { get; set; }
        public string Id { get; set; }
    }

    public class UpdateErrorController : ApiController
    {
        // POST api/updateerror
        public async Task Post(ErrUpdatePoco err)
        {
            try
            {
                try
                {
                    if (err.Id == null || err.Id == "0")
                    {
                        var cliError = JsonConvert.DeserializeObject<dynamic>(err.Error);
                        var errorString = new StringBuilder();
                        foreach (JProperty item in cliError)
                            errorString.AppendFormat("{0}: {1}<br>", item.Name, item.Value.ToString().Replace("\n", "<br>\n"));

                        var innerEx = new Exception(errorString.ToString()) {Source = "Javascript (client)"};
                        await Logging.WriteDebugInfoToErrorLog("A client-side error occured", innerEx, new RequestDTO(HttpContext.Current), err.Comment);
                    }
                    else
                    {

                        var errbl = new ErrorItemBL();
                        await errbl.UpdateErrorItemUserComment(err.Id, err.Comment);
                        errbl = null;
                    }
                }
                catch (Exception ex)
                {
                    //Writing to node WEL
                    Logging.WriteToAppLog("Unable to save user comments. \r\nError: " + ex.Message + ". \r\nComment: " + err.Comment, System.Diagnostics.EventLogEntryType.Error);
                }
            }
            catch (Exception)
            {
                //not a biggie, we have to pick our battles here...
                HttpContext.Current.ClearError();
            }
        }
    }
}
