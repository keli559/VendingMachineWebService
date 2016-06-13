using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MSData
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ITestService" in both code and config file together.
    [ServiceContract(Namespace = "http://vendnovation.com/"), XmlSerializerFormat]
    public interface ITestService
    {
        [OperationContract(Action = "http://vendnovation.com/AuthenticateCustomer")]
        AuthenticateCustomerResult AuthenticateCustomer(AuthenticateCustomerRequest request);
    }
}
