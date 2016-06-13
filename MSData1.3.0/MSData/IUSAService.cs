using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.ServiceModel.Activation;

namespace MSData
{
    //[ServiceContract(Namespace = "http://pos.usatech.com/"), XmlSerializerFormat]
    [ServiceContract(Namespace = "http://pos.usatech.com/")]
    interface IUSAService
    {
        [OperationContract(Action = "http://pos.usatech.com/Authorize")]
        USAAuthResponse Authorize(int transactionID, DateTime transactionDateTime, string terminalName, string cardNumber, string currency, int amount);

        [OperationContract(Action = "http://pos.usatech.com/Settle")]
        USASettleResponse Settle(int transactionID, DateTime transactionDateTime, string terminalName, string cardNumber, string currency, int amount, string approvalCode);

        [OperationContract(Action = "http://pos.usatech.com/Refund")]
        USASettleResponse Refund(int transactionID, DateTime transactionDateTime, string terminalName, string cardNumber, string currency, int amount);
    }
}
