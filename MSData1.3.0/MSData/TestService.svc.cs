using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MSData
{
    public class TestService : ITestService
    {
        public AuthenticateCustomerResult AuthenticateCustomer(AuthenticateCustomerRequest request)
        {
            AuthenticateCustomerResult response = new AuthenticateCustomerResult();
            //test
            // This is to test the MachineID, SiteId, UserCredentials, and CredentialTypes are receiving
            // the correct corresponding attributes from request.
            // 
            // Notice: the order of request in xml matters! 
            // The order of attributes are alphabetical.
            // In this case:
            // CredentialType, MachineID, SiteID, and UserCredential
            // but class "request" in practical won't pass in the correct order,
            //
            // to tell system to neglect the order,
            // go to IVNService.cs
            // replace with "[ServiceContract(Namespace = "http://vendnovation.com/"), XmlSerializerFormat]" as the first line
            // for [ServiceContract(Namespace = "http://vendnovation.com/")]
            // "XmlSerializerFormat" would tell system to neglect the default alphabetical order
            // this will make the testing tool from Vendnovation to work
            // K:\Curly\TaskApps\VendNovationToolkitForTesting
            // but WcfTestClient would expect a string of xml,
            //
            // to tell system to maintain the order,
            // go to IVNService.cs
            // replace with "[ServiceContract(Namespace = "http://vendnovation.com/")]" as the first line
            // for "[ServiceContract(Namespace = "http://vendnovation.com/"), XmlSerializerFormat]"
            // the default alphabetical order would be set.
            // this will make WcfTestClient to expect a list of individual attributes, not a string of xml for all.
            // but  the testing tool from Vendnovation wouldn't work.
            // K:\Curly\TaskApps\VendNovationToolkitForTesting
            // as the testing toolkit provides with an attribute order different from the default alphabetical one on request.
            LogDump.Log("MachineID: " + request.MachineID);
            LogDump.Log("SiteID: " + request.SiteID);
            LogDump.Log("UserCredentials: " + request.UserCredentials);
            LogDump.Log("CredentialType: " + request.CredentialType);
            response.Error += "<br>" + LogDump.LogContentString;
            LogDump.clearLog();
            //test
            Mailer.send("accountsetup@mykidsspending.com", "curly@odin-inc.com", null, "Test from VN, Test auth Customer", response.Error);
            return new AuthenticateCustomerResult();
        }
    }
}
