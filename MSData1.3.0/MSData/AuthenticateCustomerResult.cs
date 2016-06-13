using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MSData
{
    [DataContract(Namespace = "http://vendnovation.com/")]
    public class AuthenticateCustomerResult
    {
        bool approved = false;
        double balance = 0;
        double minimumBalance = 0;
        bool hideBalance = false;
        bool controlBalance = false;
        // testing
        string pinNumber = "";
        string pinSalt = "";
        //
        bool changePIN = false;
        bool deposits = false;
        // to disable split payment, set allowCash false
        bool allowCash = false;
        string firstName = "";
        string lastName = "";
        bool loyaltyCards = false;
        string error = "";

        [DataMember]
        public bool Approved
        {
            get { return approved; }
            set { approved = value; }
        }

        [DataMember]
        public double Balance
        {
            get { return balance; }
            set { balance = value; }
        }

        [DataMember]
        public double MinimumBalance
        {
            get { return minimumBalance; }
            set { minimumBalance = value; }
        }

        [DataMember]
        public bool HideBalance
        {
            get { return hideBalance; }
            set { hideBalance = value; }
        }

        [DataMember]
        public bool ControlBalance
        {
            get { return controlBalance; }
            set { controlBalance = value; }
        }

        [DataMember]
        public string PINNumber
        {
            get { return pinNumber; }
            set { pinNumber = value; }
        }

        [DataMember]
        public string PINSalt
        {
            get { return pinSalt; }
            set { pinSalt = value; }
        }

        [DataMember]
        public bool ChangePIN
        {
            get { return changePIN; }
            set { changePIN = value; }
        }

        [DataMember]
        public bool Deposits
        {
            get { return deposits; }
            set { deposits = value; }
        }

        [DataMember]
        public bool AllowCash
        {
            get { return allowCash; }
            set { allowCash = value; }
        }

        [DataMember]
        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        [DataMember]
        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        [DataMember]
        public bool LoyaltyCards
        {
            get { return loyaltyCards; }
            set { loyaltyCards = value; }
        }

        [DataMember]
        public string Error
        {
            get { return error; }
            set { error = value; }
        }
    }
}