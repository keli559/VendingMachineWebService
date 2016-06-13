using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Web;
using System.Text;

namespace MSData
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "NoAjaxService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select NoAjaxService.svc or NoAjaxService.svc.cs at the Solution Explorer and start debugging.
    public class NoAjaxService : INoAjaxService
    {
        public string AdjustBalance(string Uid, string Adjustment)
        {
            //WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string current = SQL.findScalar("student", Uid, "present");
            string target = Convert.ToString(Convert.ToDouble(current) + Convert.ToDouble(Adjustment));
            return SQL.update("student", "present", target, Uid) ? "OK" : "NOK";
        }
    }
}
