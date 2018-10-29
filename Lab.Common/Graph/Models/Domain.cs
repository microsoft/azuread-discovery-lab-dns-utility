using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Graph.Models
{
    public class Domain
    {
        public string authenticationType;
        public string availabilityStatus;
        public string id;
        public bool isAdminManaged;
        public bool isDefault;
        public bool isInitial;
        public bool isRoot;
        public bool isVerified;
        public DomainState state;
        public string[] supportedServices;
    }

    public class DomainState
    {
        public string lastActionDateTime;
        public string operation;
        public string status;
    }
}
