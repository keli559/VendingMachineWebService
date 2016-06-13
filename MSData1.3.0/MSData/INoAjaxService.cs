using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace MSData
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "INoAjaxService" in both code and config file together.
    [ServiceContract]
    public interface INoAjaxService
    {
        [OperationContract]
        [WebInvoke(Method = "GET")]
        string AdjustBalance(string Uid, string Adjustment);

    }
}
