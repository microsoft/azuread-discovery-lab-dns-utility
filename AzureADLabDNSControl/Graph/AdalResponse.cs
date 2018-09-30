using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;

namespace Graph
{
    public class AdalResponse
    {
        public string ResponseContent { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public bool Successful { get; set; }
        public string Message { get; set; }
        public HttpResponseMessage RawResponse { get; set; }
    }

    public class AdalResponse<T> : AdalResponse where T : class
    {
        public T Object { get; set; }

        public AdalResponse(AdalResponse baseResponse)
        {
            this.Message = baseResponse.Message;
            this.RawResponse = baseResponse.RawResponse;
            this.ResponseContent = baseResponse.ResponseContent;
            this.StatusCode = baseResponse.StatusCode;
            this.Successful = baseResponse.Successful;
            this.Object = JsonConvert.DeserializeObject<T>(baseResponse.ResponseContent);
        }
    }
}