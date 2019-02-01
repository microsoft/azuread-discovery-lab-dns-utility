using Graph;
using Graph.Models;
using Lab.Common;
using Lab.Common.Repo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Lab.Common.Infra
{
    /// <summary>
    /// Call static CreateAsync to get a new instance of this class
    /// </summary>
    public sealed class AADLinkControl
    {
        private readonly string _tenantId;
        private string _accessToken;
        private readonly HttpContextBase _hctx;

        private AADLinkControl(string tenantId, HttpContextBase hctx)
        {
            _tenantId = tenantId;
            _hctx = hctx;
        }
        private async Task<AADLinkControl> InitializeAsync()
        {
            _accessToken = await AdalLib.GetAccessToken(_hctx, _tenantId);
            return this;
        }
        public static Task<AADLinkControl> CreateAsync(string tenantId, HttpContextBase hctx)
        {
            var res = new AADLinkControl(tenantId, hctx);
            return res.InitializeAsync();
        }

        /// <summary>
        /// Add Team code and Lab code as custom attributes of the logged-in user
        /// </summary>
        /// <returns></returns>
        public async Task<AdalResponse> LinkUserToTeam(string oid, string teamCode, string labCode)
        {
            var url=string.Format("https://graph.microsoft.com/v1.0/users/{0}",oid);
            var data = new AttributeUpdate
            {
                microsoft_teamcodes = new MicrosoftTeamcodes
                {
                    labCode = labCode,
                    teamCode = teamCode
                }
            };
            string body = JsonConvert.SerializeObject(data);
            return await AdalLib.GetResourceAsync(url, _accessToken, new HttpMethod("PATCH"), body);
        }

        public async Task<MicrosoftTeamcodes> GetCodes(string oid)
        {
            var url = string.Format("https://graph.microsoft.com/v1.0/users/{0}?$select=displayName,id,description,microsoft_teamcodes", oid);
            var res = await AdalLib.GetResourceAsync(url, _accessToken, HttpMethod.Get);
            return JsonConvert.DeserializeObject<AttributeUpdate>(res.ResponseContent).microsoft_teamcodes;
        }

        /// <summary>
        /// Get the list of custom domains validated for this tenant
        /// </summary>
        /// <returns></returns>
        public async Task<AdalResponse<IEnumerable<Domain>>> GetDomains()
        {
            var url = "https://graph.microsoft.com/v1.0/domains";
            var res = await AdalLib.GetResourceAsync(url, _tenantId, _hctx, HttpMethod.Get);
            return new AdalResponse<IEnumerable<Domain>>(res);
        }

        /// <summary>
        /// If available, retrieve a single validated domain for this tenant
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public async Task<AdalResponse<Domain>> GetDomain(string domain)
        {
            var url = string.Format("https://graph.microsoft.com/v1.0/domains/{0}", domain);
            var res = await AdalLib.GetResourceAsync(url, _accessToken, HttpMethod.Get);
            return new AdalResponse<Domain>(res);
        }

        /// <summary>
        /// Remove a validated domain from this tenant
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public async Task<AdalResponse> DeleteDomain(string domain)
        {
            var url = string.Format("https://graph.microsoft.com/v1.0/domains/{0}", domain);
            var res = await AdalLib.GetResourceAsync(url, _accessToken, HttpMethod.Delete);
            if (!res.Successful)
            {
                var err = new AdalResponse<DomainError>(res);
                
                //test to see if the error is around dependency objects
                if (err.Object.Error.Details.Count() > 0)
                {
                    if (err.Object.Error.Details.Any(d => d.Code == "ObjectInUse"))
                    {
                        res.Message = "ObjectInUse";
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Get all objects dependent on a validated domain in this tenant
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public async Task<AdalResponse<DirectoryObjects>> GetDomainReferences(string domain)
        {
            var url = string.Format("https://graph.microsoft.com/v1.0/domains/{0}/domainNameReferences", domain);
            var res = await AdalLib.GetResourceAsync(url, _accessToken, HttpMethod.Get);
            return new AdalResponse<DirectoryObjects>(res);
        }

        public async Task<AdalResponse> DeleteObject(string id)
        {
            var url = string.Format("https://graph.microsoft.com/v1.0/directoryObjects/{0}", id);
            var res = await AdalLib.GetResourceAsync(url, _accessToken, HttpMethod.Delete);
            return res;
        }

        public async Task InvalidateDomain(string domainName)
        {
            var refs = await GetDomainReferences(domainName);
            //var delRes = await DeleteDomain(domainName);
        }

        public async Task<IEnumerable<DeleteError>> InvalidateAllValidatedDomains(string labCode)
        {
            var list = await LabRepo.GetDomAssignments(labCode);
            var errList = new List<DeleteError>();
            AdalResponse delRes = null;
            foreach(var item in list)
            {
                if (item.AssignedTenantId != null)
                {
                    delRes = await DeleteDomain(item.DomainName);
                    if (!delRes.Successful)
                    {
                        errList.Add(new DeleteError
                        {
                            DomainName = item.DomainName,
                            ErrorMessage = delRes.Message,
                            Response = delRes,
                            TenantId = item.AssignedTenantId
                        });
                    }
                }
            }
            return errList;
        }
    }

    public class AttributeUpdate
    {
        public MicrosoftTeamcodes microsoft_teamcodes;
    }
    public class MicrosoftTeamcodes
    {
        public string teamCode;
        public string labCode;
    }
    public class DeleteError
    {
        public string DomainName;
        public string TenantId;
        public AdalResponse Response;
        public string ErrorMessage;
    }
}