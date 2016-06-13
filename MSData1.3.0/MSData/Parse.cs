using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Data;

namespace MSData
{
    public static class Parse
    {
        public static string SearchParamsHolders(Dictionary<string, string> search_params)
        {
            return Holders(search_params, " AND ");
        }

        public static string InsertParamsHolders(Dictionary<string, string> insert_params)
        {
            return Holders(insert_params, ", ");
        }

        public static string Holders(Dictionary<string, string> insert_params, string connector)
        {
            List<string> holders = new List<string>();
            foreach (KeyValuePair<string, string> entry in insert_params)
            {
                holders.Add(entry.Key + "=@" + entry.Key);
            }
            return string.Join(connector, holders);
        }

        public static MySqlCommand InjectParams(MySqlCommand cmd, Dictionary<string, string> input_params)
        {
            foreach (KeyValuePair<string, string> entry in input_params)
            {
                cmd.Parameters.AddWithValue("@" + entry.Key, entry.Value);
            }
            return cmd;
        }

        public static string OutputParams(string[] output_params)
        {
            string response = string.Join(", ", output_params);
            return response.Length != 0 ? response : "*";
        }

        public static Dictionary<string, string> ReadResults(string[] read, string[] fields)
        {
            Dictionary<string, string> output = new Dictionary<string, string>() { };
            for (int i = 0; i < fields.Length; i++)
            {
                output.Add(fields[i], read[i]);
            }
            return output;
        }

        public static string readSchool(string input)
        {
            if (input == "" || input == null) { return ""; }
            else { return Regex.Split(input, @"\d+")[0]; }
        }

        public static string findPayType(Dictionary<string, string> account)
        {
            if (Convert.ToBoolean(account["charge"])) { return account["chgpaytype"]; }
            else if (Convert.ToBoolean(account["debit"])) { return account["dbtpaytype"]; }
            else { return ""; }
        }

        public static string findType(string debit, string maintnbal)
        {
            return !Convert.ToBoolean(debit) && Convert.ToBoolean(maintnbal) ? "Pmt" : "Dep";
        }

        public static string giveDate(DateTime time)
        {
            return time.Year + "-" + time.Month + "-" + time.Day;
        }

        public static string truncNumb(string input)
        {
            return input != "" ? Convert.ToDouble(input).ToString() : "0";
        }

        public static string truncTime(string input)
        {
            DateTime time = Convert.ToDateTime(input);
            return truncTime(time);
        }

        public static string truncTime(DateTime time)
        {
            return Convert.ToString(time.Month) + "/" + Convert.ToString(time.Day) + "/" + Convert.ToString(time.Year);
        }

        public static Dictionary<string, string> stringToDictionary(string input)
        {
            Dictionary<string, string> response = new Dictionary<string, string>() { };
            while (input.Length > 0)
            {
                string[] first = getFirstPair(input);
                if (first[0] != null) { response.Add(first[0], first[1]); }
                input = first[2];
            }
            return response;
        }

        public static string[] getFirstPair(string input)
        {
            string key = Regex.Match(input, "<.+?>").Value;
            if (key.Length == 0) { return new string[] { null, null, "" }; }
            input = input.Remove(0, key.Length);
            key = key.Remove(key.Length - 1, 1).Remove(0, 1);
            string value = Regex.Match(input, ".*?(?=<)").Value;
            input = input.Remove(0, value.Length + key.Length + 3);
            return new string[] { key, value, input };
        }

        public static string shave(string input)
        {
            string response = input;
            response = Regex.Replace(response, "'", "");
            response = Regex.Replace(response, @"""", "");
            response = Regex.Replace(response, @"\(", "");
            response = Regex.Replace(response, @"\)", "");
            return response.Trim();
        }

        public static string toXML(Dictionary<string, string> xml)
        {
            string response = "";
            foreach (string key in xml.Keys)
            {
                response += "<" + key + ">" + xml[key] + "<" + key + "/>";
            }
            return response;
        }
    }
}