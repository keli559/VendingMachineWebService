using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MSData
{
    [DataContract(Namespace = "http://vendnovation.com/")]
    public class SaleCompletedRequest
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
        public string LoyaltyCard { get; set; }
        [DataMember]
        public string LoyaltyType { get; set; }
        [DataMember]
        public string Selection { get; set; }
        [DataMember]
        public string ProductID { get; set; }
        [DataMember]
        public double Price { get; set; }
        [DataMember]
        public double AccountFundsUsed { get; set; }
        [DataMember]
        public double CashFundsUsed { get; set; }
        [DataMember]
        public string InvoiceNumber { get; set; }
        [DataMember]
        public DateTime Time { get; set; }
    }
}