using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lab.Common
{
    public static class CustomClaimTypes
    {
        public const string AuthType = "AuthType";
        public const string FullName = "FullName";
        public const string LabCode = "LabCode";
        public const string TeamCode = "TeamCode";
        public const string TenantName = "TenantName";
    }

    public static class CustomRoles
    {
        public const string LabAdmin = "LabAdmin";
        public const string LabUser = "LabUser";
        public const string LabUserAssigned = "LabUserAssigned";
        public const string SiteAdmin = "SiteAdmin";
    }

    public static class CustomAuthType
    {
        public static readonly string LabAdmin = "LabAdmin";
        public static readonly string LabUser = "LabUser";
    }
}