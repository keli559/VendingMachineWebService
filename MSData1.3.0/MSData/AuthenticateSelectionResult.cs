using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MSData
{
    [DataContract(Namespace = "http://vendnovation.com/")]
    public class AuthenticateSelectionResult
    {
        bool approved = false;
        bool depleteCashFirst = true;
        string invoiceNumber = "";
        double newPrice = 0;
        string errorMessageHigh = "";
        string errorMessageLow = "";
        string error = "";

        [DataMember]
        public bool Approved
        {
            get { return approved; }
            set { approved = value; }
        }

        [DataMember]
        public bool DepleteCashFirst
        {
            get { return depleteCashFirst; }
            set { depleteCashFirst = value; }
        }

        [DataMember]
        public string InvoiceNumber
        {
            get { return invoiceNumber; }
            set { invoiceNumber = value; }
        }

        [DataMember]
        public double NewPrice
        {
            get { return newPrice; }
            set { newPrice = value; }
        }

        [DataMember]
        public string ErrorMessageHigh
        {
            get { return errorMessageHigh; }
            set { errorMessageHigh = value; }
        }

        [DataMember]
        public string ErrorMessageLow
        {
            get { return errorMessageLow; }
            set { errorMessageLow = value; }
        }

        [DataMember]
        public string Error
        {
            get { return error; }
            set { error = value; }
        }
    }
}