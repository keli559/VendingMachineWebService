using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace MSData
{
    //used to Balance(). For school other than NMH and CCS the function will calculate current balance from deposits minus spending.
    public static class Calc
    {
        public static double spending(string id, string school, string payType)
        {
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"id_number", id},
                {"school", school},
                {"payment", payType}
            };
            //find sum of expenditures with the appropriate paytype.
            return SQL.findSum("transact", search_params, "amount");
        }

        public static double deposits(string id, string school, string payType, string type)
        {
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"id_number", id},
                {"school", school},
                {"payment", payType},
                {"type", type}
            };
            //find the sum of deposits with the appropriate type and payment type.
            return SQL.findSum("transfer", search_params, "amount");
        }

        public static double balance(string id, string school)
        {
            //find the appripriate payment type for the student.
            string[] payType = SQL.findPayType(id, school);
            string type = Parse.findType(payType[1], payType[3]);

            //return deposits - spending
            Debug.WriteLine(deposits(id, school, payType[0], type).ToString());
            Debug.WriteLine(spending(id, school, payType[0]).ToString());
            return deposits(id, school, payType[0], type) - spending(id, school, payType[0]);
        }
    }
}