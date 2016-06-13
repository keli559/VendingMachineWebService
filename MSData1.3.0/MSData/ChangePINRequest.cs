using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MSData
{
    [DataContract(Namespace = "http://vendnovation.com/")]
    public class ChangePINRequest
    {
        public string MachineID { get; set; }
        public string SiteID { get; set; }
        public string UserCredentials { get; set; }
        public string CredentialType { get; set; }
        public string PIN { get; set; }
    }
}