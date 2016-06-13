using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSData;

namespace MSDataUnitTests
{
    [TestClass]
    public class ApproveUnitTest
    {
        [TestMethod]
        public void withAllow_rejectsInsufficientTotalFunds()
        {
            Assert.IsFalse(Approve.allowance(true, 0, 2, 0, 100, 0, 100, 0).Approved);
        }

        [TestMethod]
        public void withAllow_rejectsInsufficientTotalFundsPlusCash()
        {
            Assert.IsFalse(Approve.allowance(true, 0, 2, 1, 100, 0, 100, 0).Approved);
        }

        [TestMethod]
        public void withAllow_rejectsInsufficientTotalFunds_givesCorrectError()
        {
            Assert.AreEqual("Insufficient funds.", Approve.allowance(true, 0, 2, 0, 100, 0, 100, 0).Message);
        }

        [TestMethod]
        public void withAllow_AllowsCashToMakeUpTotalDifference()
        {
            Assert.IsTrue(Approve.allowance(true, 0, 2, 3, 100, 0, 100, 0).Approved);
        }

        [TestMethod]
        public void withAllow_rejectsInsufficientWeeklyFunds()
        {
            Assert.IsFalse(Approve.allowance(true, 100, 2, 0, 20, 25, 100, 0).Approved);
        }

        [TestMethod]
        public void withAllow_rejectsInsufficientWeeklyFunds_givesCorrectError()
        {
            Assert.AreEqual("Insufficient weekly allowance.", Approve.allowance(true, 100, 2, 0, 20, 25, 100, 0).Message);
        }

        [TestMethod]
        public void withAllow_allowsCashToMakeUpWeeklyDifference()
        {
            Assert.IsTrue(Approve.allowance(true, 100, 2, 10, 20, 25, 100, 0).Approved);
        }

        [TestMethod]
        public void withAllow_treatsZeroWeeklyAllowAsUnlimited()
        {
            Assert.IsTrue(Approve.allowance(true, 100, 2, 0, 0, 25, 100, 0).Approved);
        }

        [TestMethod]
        public void withAllow_rejectsInsufficientMonthlyFunds()
        {
            Assert.IsFalse(Approve.allowance(true, 100, 2, 0, 100, 0, 20, 25).Approved);
        }

        [TestMethod]
        public void withAllow_rejectsInsufficientMonthlyFunds_givesCorrectError()
        {
            Assert.AreEqual("Insufficient monthly allowance.", Approve.allowance(true, 100, 2, 0, 100, 0, 20, 25).Message);
        }

        [TestMethod]
        public void withAllow_allowsCashToMakeUpMonthlyFunds()
        {
            Assert.IsTrue(Approve.allowance(true, 100, 2, 10, 100, 0, 20, 25).Approved);
        }

        [TestMethod]
        public void withAllow_treatsZeroMonthlyAllowAsUnlimited()
        {
            Assert.IsTrue(Approve.allowance(true, 100, 2, 0, 100, 0, 0, 25).Approved);
        }

        [TestMethod]
        public void withAllow_rejectsInactive()
        {
            Assert.IsFalse(Approve.allowance(false, 100, 2, 0, 100, 0, 100, 0).Approved);
        }

        [TestMethod]
        public void withAllow_rejectsInactive_givesCorrectError()
        {
            Assert.AreEqual("Not an active account.", Approve.allowance(false, 100, 2, 0, 100, 0, 100, 0).Message);
        }

        [TestMethod]
        public void withAllow_approvesWhenNoIssues()
        {
            Assert.IsTrue(Approve.allowance(true, 100, 5, 3, 100, 2, 400, 6).Approved);
        }

        [TestMethod]
        public void withAllow_approvalGivesNoError()
        {
            Assert.AreEqual("", Approve.allowance(true, 100, 5, 3, 100, 2, 400, 6).Message);
        }
    }
}