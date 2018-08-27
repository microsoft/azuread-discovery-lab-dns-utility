using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocDBLib
{
    /// <summary>
    /// Filter for DocumentDB doc types - any records stored in DocumentDB need their type names added here
    /// </summary>
    public enum DocTypes
    {
        PreAuthDomain,
        GuestRequest,
        SiteConfig,
        InviteTemplate
    }
}