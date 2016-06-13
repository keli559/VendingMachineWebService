using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MSData
{
    [DataContract(Namespace = "http://vendnovation.com/")]
    public class AuthenticateCustomerRequest
    {
        [DataMember]
        public string MachineID { get; set; }
        [DataMember]
        public string SiteID { get; set; }
        [DataMember]
        public string UserCredentials { get; set; }
        [DataMember]
        public string CredentialType { get; set; }
    }
}