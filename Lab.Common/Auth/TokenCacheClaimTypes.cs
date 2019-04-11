using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Infra.Auth
{
    public static class TokenCacheClaimTypes
    {
        public static string IdentityProvider = "http://schemas.microsoft.com/identity/claims/identityprovider";
        public static string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";
        public static string ObjectId = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    }
}