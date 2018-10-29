using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lab.Common
{
    public static class CustomClaimTypes
    {
        public const string IdentityProvider = "http://schemas.microsoft.com/identity/claims/identityprovider";
        public const string AuthType = "AuthType";
        public const string FullName = "FullName";
        public const string ObjectIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";
        public const string LabCode = "LabCode";
        public const string TeamCode = "TeamCode";
    }

    public static class CustomRoles
    {
        public const string LabAdmin = "LabAdmin";
        public const string LabUser = "LabUser";
        public const string LabUserAssigned = "LabUserAssigned";
    }

    public static class CustomAuthType
    {
        public static readonly string LabAdmin = "LabAdmin";
        public static readonly string LabUser = "LabUser";
    }
}