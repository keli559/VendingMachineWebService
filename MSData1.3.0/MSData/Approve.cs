using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MSData
{
    public static class Approve
    {
        //used to check if a user is approved for a purchase.
        //for USA, which does not allow cash, cash will be input as 0.
        public static ApproveWithMsg allowance(bool lactive, double present, double price, double cash, double wkly_allow, double wkly_bal, double mnth_allow, double mnth_bal)
        {
            ApproveWithMsg response = new ApproveWithMsg();

            //Flag if user doesn't exist or is inactive
            if (!lactive) { response.message = "Not an active account."; }

            //check if enough balance to purchase item
            else if (price - cash > present) { response.Message = "Insufficient funds."; }

            //check if weekly allowance is enough to purchase item
            else if (wkly_allow != 0 && price - cash + wkly_bal > wkly_allow) { response.Message = "Insufficient weekly allowance."; }

            //check if monthly balance is enough to purchase item
            else if (mnth_allow != 0 && price - cash + mnth_bal > mnth_allow) { response.Message = "Insufficient monthly allowance."; }
            else { response.approved = true; }

            return response;
        }
    }

    //class used in method above, allows ApproveWithMsg() to return a bool and a string
    public class ApproveWithMsg
    {
        public bool approved = false;
        public string message = "";

        public bool Approved
        {
            get { return approved; }
            set { approved = value; }
        }
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}