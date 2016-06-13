using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.ServiceModel.Activation;
using MSData.MksServiceReference1;

namespace MSData
{
    //hit F5 from this file to bring up test client
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Vending : IVNService, IUSAService
    {

        //first method sent by VendNovation. Checks if the given student is allow to make vending purchases
        //will be rejected if: student not found in system or student is marked inactive
        public AuthenticateCustomerResult AuthenticateCustomer(AuthenticateCustomerRequest request)
        {// under request parameters, 
            //  UserCredentials ---> student id_number
            //  SiteID ------------> school name (school column in student table)
            //  
            //I beleive SiteID will be the school name and machineID wil be the number of the unit in the school.
            AuthenticateCustomerResult response = new AuthenticateCustomerResult();
            if (request == null) { response.Error = "No Data"; response.Approved = false; return response; }
            string[] output_params = { "lactive", "present", "student", "last_name" , "pin"};
            Dictionary<string, string> outputs = SQL.findRow("student", request.UserCredentials,
                Parse.readSchool(request.SiteID), output_params);
            //Dictionary<string, string> outputs = SQL.findRow("student", userCredentials,
                //Parse.readSchool(machineID), output_params);
            response.Approved = Convert.ToBoolean(outputs["lactive"]);
            if (!response.Approved) { response.Error = "Not an active account."; }
            response.Balance = Convert.ToDouble(outputs["present"]);
            response.FirstName = outputs["student"];
            response.LastName = outputs["last_name"];
            response.PINNumber = outputs["pin"];

            //test
            LogDump.Log("UserCredentials: " + request.UserCredentials);
            LogDump.Log("MachineID: " + request.MachineID);
            LogDump.Log("CredentialType: " + request.CredentialType);
            LogDump.Log("SiteID: " + request.SiteID);
            response.Error += "<br>" + LogDump.LogContentString;
            LogDump.clearLog();
            //test

            Mailer.send("accountsetup@mykidsspending.com", "curly@odin-inc.com", null, "Test from VN, auth Customer", response.Error);
            return response;
        }

        //sent by VendNovation after AuthenticateCustomer
        //checks if student is allow to purchase the specified item.
        //will be rejected if: student is not found, student is marked inactive, student balance is too low,
        //remaining weekly allowance is too low, or remaining monthly balnace is too low.
        //Student can use cash to supplement account funds.
        public AuthenticateSelectionResult AuthenticateSelection(AuthenticateSelectionRequest request)
        {
            AuthenticateSelectionResult response = new AuthenticateSelectionResult();
            if (request == null) { response.Error = "No Data"; response.Approved = false; return response; }
            response.NewPrice = request.Price;
            string[] output_params = { "lactive", "present", "wkly_allow", "wkly_bal", "mnth_allow", "mnth_bal" };
            Dictionary<string, string> outputs = SQL.findRow("student", request.UserCredentials,
                Parse.readSchool(request.SiteID), output_params);
            bool lactive = Convert.ToBoolean(outputs["lactive"]);
            double present = Convert.ToDouble(outputs["present"]);
            double wkly_allow = Convert.ToDouble(outputs["wkly_allow"]);
            double wkly_bal = Convert.ToDouble(outputs["wkly_bal"]);
            double mnth_allow = Convert.ToDouble(outputs["mnth_allow"]);
            double mnth_bal = Convert.ToDouble(outputs["mnth_bal"]);
            ApproveWithMsg app = Approve.allowance(lactive, present, request.Price, request.CashBalance, wkly_allow, wkly_bal, mnth_allow, mnth_bal);
            response.Approved = app.Approved;
            response.Error = app.Message;
            //test
            LogDump.Log("UserCredentials: " + request.UserCredentials);
            LogDump.Log("MachineID: " + request.MachineID);
            LogDump.Log("AccountBalance: " + request.AccountBalance);
            LogDump.Log("CashBalance: " + request.CashBalance);
            LogDump.Log("CredentialType: " + request.CredentialType);
            LogDump.Log("ProductID: " + request.ProductID);
            LogDump.Log("SiteID: " + request.SiteID);
            LogDump.Log("Price: " + request.Price);
            LogDump.Log("Selection: " + request.Selection);
            LogDump.Log("LoyaltyCard: " + request.LoyaltyCard);
            LogDump.Log("LoyaltyType: " + request.LoyaltyType);
            response.Error += "<br>" + LogDump.LogContentString;
            LogDump.clearLog();
            //test
            Mailer.send("accountsetup@mykidsspending.com", "curly@odin-inc.com", null, "Test from VN, auth Selection", response.Error);
            return response;
        }

        //sent by VendNovation after AuthenticateSelection if sale successful
        //creates a record in MKSData.transact
        public SaleCompletedResult SaleCompleted(SaleCompletedRequest request)
        {
            SaleCompletedResult response = new SaleCompletedResult();
            if (request == null) { response.Error = "No Data"; response.Approved = false; return response; }

            //Mailer.send("accountsetup@mykidsspending.com", "curly@odin-inc.com", null, "Test from VN, SaleCompleted", "Sale Completed");

            string payType = SQL.findPayType(request.UserCredentials, Parse.readSchool(request.SiteID))[0];
            if (payType == "")
            {
                response.Error = "Invalid Pay Type.";
                return response;
            }
            bool created = SQL.createVendingTransaction(
                request.UserCredentials, request.MachineID, 
                request.Time, request.ProductID, 
                request.AccountFundsUsed, payType, 
                request.InvoiceNumber, 
                request.SiteID);
            response.Approved = created;
            if (created)
            {
                response.Error = "Sales completed!";

                NoAjaxServiceClient MksServiceInstance = new NoAjaxServiceClient();
                string uid = SQL.findScalar("student", request.UserCredentials.ToString(), request.SiteID.ToString(), "uid");
                MksServiceInstance.AdjustBalance(uid, (request.AccountFundsUsed*(-1)).ToString());
            }
            else { response.Error = "Unable to create transaction"; }
            //test
            LogDump.Log("UserCredentials: " + request.UserCredentials);
            LogDump.Log("MachineID: " + request.MachineID);
            LogDump.Log("Time: " + request.Time);
            LogDump.Log("ProductID: " + request.ProductID);
            LogDump.Log("AccountFundUsed: " + request.AccountFundsUsed);
            LogDump.Log("PayType: " + payType);
            LogDump.Log("InvoiceNumber: " + request.InvoiceNumber);
            LogDump.Log("SiteID: " + request.SiteID);
            response.Error += "<br>" + LogDump.LogContentString;
            LogDump.clearLog();
            //test
            Mailer.send("accountsetup@mykidsspending.com", 
                "curly@odin-inc.com", null, "Test from VN, SaleCompleted", 
                response.Error);
            return response;
        }

        //sent by VendNovation after AuthenticateSelection if sale failed
        //no record in created
        public SaleAbortedResult SaleAborted(SaleAbortedRequest request)
        {
            //return message if the sale is cancelled on the machine's side.
            SaleAbortedResult response = new SaleAbortedResult();
            response.Approved = true;
            response.Error = "Sales Aborted.";
            //test
            LogDump.Log("UserCredentials: " + request.UserCredentials);
            LogDump.Log("AccountFundUsed: " + request.AccountFundsUsed);
            LogDump.Log("CashFundUsed: " + request.CashFundsUsed);
            LogDump.Log("Price: " + request.Price);
            LogDump.Log("MachineID: " + request.MachineID);
            LogDump.Log("Time: " + request.Time);
            LogDump.Log("ProductID: " + request.ProductID);

            LogDump.Log("Selection: " + request.Selection);
            LogDump.Log("CredentialType: " + request.CredentialType);
            LogDump.Log("InvoiceNumber: " + request.InvoiceNumber);
            LogDump.Log("SiteID: " + request.SiteID);
            response.Error += "<br>" + LogDump.LogContentString;
            LogDump.clearLog();
            //test
            Mailer.send("accountsetup@mykidsspending.com",
                "curly@odin-inc.com", null, "Test from VN, SaleAborted",
                response.Error);
            return response;
        }

        //VN allows for money to be deposited in the machine and applied to the student's account.
        //Right now we do not offer this feature.
        public DepositCompleteResult DepositComplete(DepositCompleteRequest request)
        {
            DepositCompleteResult response = new DepositCompleteResult();
            response.Approved = false;
            response.Error = "This feature not currently enabled.";
            return response;
        }

        //VN allows for the student to change their PIN from the machine
        //Right now we do not offer this feature.
        public ChangePINResult ChangePIN(ChangePINRequest request)
        {
            ChangePINResult response = new ChangePINResult();
            response.Approved = false;
            response.Error = "This feature not currently enabled.";
            return response;
        }

        //First method called by USATech
        //Checks if student is allowed to make vending purchase and if they can purchase the specified item
        //will be rejected if student not found, student marked inactive, balance too low, remaining weekly allowance too low, remaining monthly allowance too low
        //USAT does not allow cash to supplement account balance
        public USAAuthResponse Authorize(
            int transactionID, DateTime transactionDateTime, string terminalName, 
            string cardNumber, string currency, int amount
            )
        {
            USAAuthResponse response = new USAAuthResponse();
            response.TransactionID = transactionID;
            if (currency != "USD")
            {
                response.ResponseMessage = "This system only accepts USD.";
                response.ApprovedAmount = 0;
                response.StatusCode = 1;
                return response;
            }
            string[] output_params = { "lactive", "present", "wkly_allow", "wkly_bal", "mnth_allow", "mnth_bal" };
            Dictionary<string, string> outputs = SQL.findRow("student", cardNumber, Parse.readSchool(terminalName), output_params);
            bool lactive = Convert.ToBoolean(outputs["lactive"]);
            double present = Convert.ToDouble(outputs["present"]);
            double wkly_allow = Convert.ToDouble(outputs["wkly_allow"]);
            double wkly_bal = Convert.ToDouble(outputs["wkly_bal"]);
            double mnth_allow = Convert.ToDouble(outputs["mnth_allow"]);
            double mnth_bal = Convert.ToDouble(outputs["mnth_bal"]);
            double dollarAmount = amount / 100.0;
            ApproveWithMsg app = Approve.allowance(lactive, present, dollarAmount, 0, wkly_allow, wkly_bal, mnth_allow, mnth_bal);
            response.ResponseMessage = app.message;
            //test
            response.ResponseMessage += "<br>" + LogDump.LogContentString;
            LogDump.clearLog();
            
            Mailer.send("accountsetup@mykidsspending.com",
                "curly@odin-inc.com", null, "Test from USATech, test authorize",
                response.ResponseMessage);
            //test
            if (app.approved)
            {
                response.ApprovedAmount = 275;
                response.StatusCode = 0;
                response.ResponseMessage = "Approved for $" + dollarAmount;
                return response;
            }
            response.StatusCode = 1;
            response.ResponseMessage = app.message;
            
            return response;
        }

        //method called by USATech if sale successful
        //creates a record in MSKData.transact
        public USASettleResponse Settle(
            int transactionID, DateTime transactionDateTime, 
            string terminalName, string cardNumber, string currency, 
            int amount, string approvalCode
            )
        {
            USASettleResponse response = new USASettleResponse();
            string payType = SQL.findPayType(cardNumber, Parse.readSchool(terminalName))[0];
            if (payType == "")
            {
                response.ResponseMessage = "Invalid Pay Type.";
                return response;
            }
            double dollarAmount = amount / 100.0;
            try
            {
                SQL.createVendingTransaction(cardNumber, terminalName, transactionDateTime,
                "Vending Item", dollarAmount, payType, transactionID.ToString());
                response.StatusCode = 0;
            }
            catch
            {
                response.StatusCode = 1;
            }
            
            response.TransactionID = transactionID;
            response.ResponseMessage = "Settled: $" + dollarAmount;
            return response;
        }

        //method called by USATech to refund a transaction
        //we currently do not allow this feature.
        public USASettleResponse Refund(
            int transactionID, DateTime transactionDateTime, string terminalName, 
            string cardNumber, string currency, int amount
            )
        {
            USASettleResponse response = new USASettleResponse();
            response.ResponseMessage = "Please see cafeteria manager for refunds.";
            response.StatusCode = 1;
            response.TransactionID = transactionID;
            return response;
        }
    }
}
