using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MSData
{
    [DataContract(Namespace = "http://pos.usatech.com/")]
    public class USAAuthResponse
    {
        int transactionID = 0;
        int statusCode = 2;
        string responseMessage = "";
        string approvalCode = "";
        int approvedAmount = 0;

        [DataMember]
        public int TransactionID
        {
            get { return transactionID; }
            set { transactionID = value; }
        }

        [DataMember]
        public int StatusCode
        {
            get { return statusCode; }
            set { statusCode = value; }
        }

        [DataMember]
        public string ResponseMessage
        {
            get { return responseMessage; }
            set { responseMessage = value; }
        }

        [DataMember]
        public string ApprovalCode
        {
            get { return approvalCode; }
            set { approvalCode = value; }
        }

        [DataMember]
        public int ApprovedAmount
        {
            get { return approvedAmount; }
            set { approvedAmount = value; }
        }
    }
}