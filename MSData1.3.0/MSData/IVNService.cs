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

    //[ServiceContract(Namespace = "http://vendnovation.com/")]
    [ServiceContract(Namespace = "http://vendnovation.com/"), XmlSerializerFormat]
    public interface IVNService
    {
        [OperationContract(Action = "http://vendnovation.com/AuthenticateCustomer")]
        AuthenticateCustomerResult AuthenticateCustomer(AuthenticateCustomerRequest request);

        [OperationContract(Action = "http://vendnovation.com/AuthenticateSelection")]
        AuthenticateSelectionResult AuthenticateSelection(AuthenticateSelectionRequest request);

        [OperationContract(Action = "http://vendnovation.com/SaleCompleted")]
        SaleCompletedResult SaleCompleted(SaleCompletedRequest request);

        [OperationContract(Action = "http://vendnovation.com/SaleAborted")]
        SaleAbortedResult SaleAborted(SaleAbortedRequest request);

        [OperationContract(Action = "http://vendnovation.com/DepositComplete")]
        DepositCompleteResult DepositComplete(DepositCompleteRequest request);

        [OperationContract(Action = "http://vendnovation.com/ChangePIN")]
        ChangePINResult ChangePIN(ChangePINRequest request);
    }
}
