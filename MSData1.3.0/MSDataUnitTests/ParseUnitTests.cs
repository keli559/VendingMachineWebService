using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSData;
using MySql.Data.MySqlClient;
using System.Data;

namespace MKSServiceTests
{
    [TestClass]
    public class ParseUnitTests
    {
        [TestMethod]
        public void searchParamsHolders_turnsStringArrayToInjectionString()
        {
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"id", "4"},
                {"school", "valhalla"},
                {"last_name", "Banner"}
            };
            Assert.AreEqual("id=@id AND school=@school AND last_name=@last_name", Parse.SearchParamsHolders(search_params));
        }

        [TestMethod]
        public void injectSearchParams_injectsCmdTextWithValues()
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "SELECT * FROM student WHERE id=@id AND school=@school AND last_name=@last_name;";
            Dictionary<string, string> search_params = new Dictionary<string, string>() 
            {
                {"id", "4"},
                {"school", "valhalla"},
                {"last_name", "banner"}
            };
            cmd = Parse.InjectParams(cmd, search_params);

            Assert.AreEqual("4", cmd.Parameters[0].Value.ToString());
            Assert.AreEqual("valhalla", cmd.Parameters[1].Value.ToString());
            Assert.AreEqual("banner", cmd.Parameters[2].Value.ToString());
            Assert.AreEqual<int>(3, cmd.Parameters.Count);
        }

        [TestMethod]
        public void outputParams_turnsStringArrayToConcatString()
        {
            string[] output_params = { "lactive", "last_name", "school" };
            Assert.AreEqual("lactive, last_name, school", Parse.OutputParams(output_params));
        }

        [TestMethod]
        public void outputParams_returnsAll_fromEmptyInput()
        {
            string[] output_params = { };
            Assert.AreEqual("*", Parse.OutputParams(output_params));
        }

        [TestMethod]
        public void readResults_createsADictionaryFromReaderResults()
        {
            string[] read = { "45", "true", "Banner", "No candy", "valhalla" };
            string[] fields = { "ID", "Approved", "Name", "Notes", "School" };

            Dictionary<string, string> outputDictionary = Parse.ReadResults(read, fields);
            Assert.IsTrue(5 == outputDictionary.Count);
            Assert.AreEqual("45", outputDictionary["ID"]);
            Assert.AreEqual("true", outputDictionary["Approved"]);
            Assert.AreEqual("Banner", outputDictionary["Name"]);
            Assert.AreEqual("No candy", outputDictionary["Notes"]);
            Assert.AreEqual("valhalla", outputDictionary["School"]);
        }

        [TestMethod]
        public void readSchool_shouldCutOffTrailingNumbers()
        {
            Assert.AreEqual("valhalla", Parse.readSchool("valhalla456465"));
        }

        [TestMethod]
        public void readSchool_shouldCutOffcharactersAfterNumbers()
        {
            Assert.AreEqual("valhalla", Parse.readSchool("valhalla4asdasd"));
        }

        [TestMethod]
        public void readSchool_shouldhandleEmpty()
        {
            Assert.AreEqual("", Parse.readSchool(""));
        }

        [TestMethod]
        public void findPayType_chargeTrueShouldGiveChgpaytype()
        {
            Dictionary<string, string> account = new Dictionary<string, string>()
            {
                {"charge", "true"},
                {"debit", "false"},
                {"chgpaytype", "C"},
                {"dbtpaytype", "D"}
            };
            Assert.AreEqual("C", Parse.findPayType(account));
        }

        [TestMethod]
        public void findPayType_chargeFalseDebitTrueShouldGiveDbtpaytype()
        {
            Dictionary<string, string> account = new Dictionary<string, string>()
            {
                {"charge", "false"},
                {"debit", "true"},
                {"chgpaytype", "C"},
                {"dbtpaytype", "D"}
            };
            Assert.AreEqual("D", Parse.findPayType(account));
        }

        [TestMethod]
        public void findPayType_bothFalseShouldGiveEmpty()
        {
            Dictionary<string, string> account = new Dictionary<string, string>()
            {
                {"charge", "false"},
                {"debit", "false"},
                {"chgpaytype", "C"},
                {"dbtpaytype", "D"}
            };
            Assert.AreEqual("", Parse.findPayType(account));
        }

        [TestMethod]
        public void findType_trueTrueGivesDep()
        {
            Assert.AreEqual("Dep", Parse.findType("true", "true"));
        }

        [TestMethod]
        public void findType_trueFalseGivesDep()
        {
            Assert.AreEqual("Dep", Parse.findType("true", "false"));
        }

        [TestMethod]
        public void findType_falseTrueGivesPmt()
        {
            Assert.AreEqual("Pmt", Parse.findType("false", "true"));
        }

        [TestMethod]
        public void findType_falseFalseGivesDep()
        {
            Assert.AreEqual("Dep", Parse.findType("false", "false"));
        }

        [TestMethod]
        public void giveDate_shouldConvertDateTimeToStringDateWithDashes()
        {
            DateTime time = new DateTime(2015, 11, 14);
            Assert.AreEqual("2015-11-14", Parse.giveDate(time));
        }

        [TestMethod]
        public void truncNumb_shouldConvertStringNumbToMoreReadableFormat()
        {
            Assert.AreEqual("0", Parse.truncNumb("0.00000000"));
            Assert.AreEqual("2.75", Parse.truncNumb("2.75000000"));
            Assert.AreEqual("1", Parse.truncNumb("1.00000000"));
        }

        [TestMethod]
        public void truncTime_shouldChopOffTimeFromDateTimeAndReturnDate()
        {
            DateTime time = new DateTime(2015, 11, 4, 6, 10, 40);
            Assert.AreEqual("11/4/2015", Parse.truncTime(time));
            Assert.AreEqual("11/4/2015", Parse.truncTime(Convert.ToString(time)));
        }

        [TestMethod]
        public void stringToDictionary_shouldTakeAStringAnCovertToKeyValuePairs()
        {
            string input = "<name>Thor</name><cc>1</cc><school>Valhalla</school><uid>1234-1234</uid>";
            Dictionary<string, string> data = Parse.stringToDictionary(input);
            Assert.AreEqual("Thor", data["name"]);
            Assert.AreEqual("1", data["cc"]);
            Assert.AreEqual("Valhalla", data["school"]);
            Assert.AreEqual("1234-1234", data["uid"]);
        }

        [TestMethod]
        public void shave_shouldFormatStringCorrectly()
        {
            Assert.AreEqual("a,b,c", Parse.shave("(a,b,c)"));
        }
    }
}