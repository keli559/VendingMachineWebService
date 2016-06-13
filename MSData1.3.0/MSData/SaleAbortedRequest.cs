using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MSData
{
    [DataContract(Namespace = "http://vendnovation.com/")]
    public class SaleAbortedRequest
    {
        [DataMember]
        public string MachineID { get; set; }
        [DataMember]
        public string SiteID { get; set; }
        [DataMember]
        public string UserCredentials { get; set; }
        [DataMember]
        public string CredentialType { get; set; }
        [DataMember]
        public string Selection { get; set; }
        [DataMember]
        public string ProductID { get; set; }
        [DataMember]
        public string Price { get; set; }
        [DataMember]
        public string AccountFundsUsed { get; set; }
        [DataMember]
        public double CashFundsUsed { get; set; }
        [DataMember]
        public string InvoiceNumber { get; set; }
        [DataMember]
        public DateTime Time { get; set; }
    }
}