using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;

namespace MSData
{
    public static class SQL
    {
        //calls the connection string used for most actions. Updating records requires a different string with its own permissions.
        static string mksString = ConfigurationManager.ConnectionStrings["MKSDB"].ConnectionString;
        public static MySqlConnection mksConnection = new MySqlConnection(mksString);

        //returns array with debit(0/1), credit(0/1), and maintnbal
        public static string[] findPayType(string id, string school)
        {
            string output_field = "accnt_type";
            string output = SQL.findScalar("student", id, school, output_field);
            return findPayTypeDirect(output, school);
        }

        //returns array with debit(0/1), credit(0/1), and maintnbal
        public static string[] findPayTypeDirect(string accnt_type, string school)
        {
            string[] accnt_out_params = { "charge", "debit", "chgpaytype", "dbtpaytype", "maintnbal" };
            Dictionary<string, string> accnt_search_params = new Dictionary<string, string> 
                {
                    {"accnt_type", accnt_type},
                    {"school", school}
                };
            Dictionary<string, string> accnt = SQL.findRow("accntype", accnt_search_params, accnt_out_params);
            return new string[] { Parse.findPayType(accnt), accnt["debit"], accnt["charge"], accnt["maintnbal"] };
        }

        //finds a scalar, specific case of known id and school
        public static string findScalar(string table, string id, string school, string output_field)
        {
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"id_number", id},
                {"school", school}
            };
            return findScalar(table, search_params, output_field);
        }

        //finds a scalar, specific case of known uid
        public static string findScalar(string table, string uid, string output_field)
        {
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"uid", uid}
            };
            return findScalar(table, search_params, output_field);
        }

        //find a scalar, general case
        public static string findScalar(string table, Dictionary<string, string> search_params, string output_field)
        {
            Connect.Open(mksConnection);
            MySqlCommand cmd = mksConnection.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT " + output_field + " FROM " + table + " WHERE " + Parse.SearchParamsHolders(search_params);
            cmd.Prepare();
            Parse.InjectParams(cmd, search_params);
            string result = Convert.ToString(cmd.ExecuteScalar());
            Connect.Close(mksConnection);
            return result;
        }

        //find multiple attributes in single row, specific case with known uid 
        public static Dictionary<string, string> findRow(string table, string uid, string[] output_params)
        {
            Dictionary<string, string> search_params = new Dictionary<string, string>
            {
                {"uid", uid}
            };
            return findRow(table, search_params, output_params);
        }

        //find multiple attributes in single row, specific case with known student id and school
        public static Dictionary<string, string> findRow(string table, string id, string school, string[] output_params)
        {
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"school", school},
                {"id_number", id}
            };
            return findRow(table, search_params, output_params);
        }

        //find multiple attributes in single row, general case
        public static Dictionary<string, string> findRow(string table, Dictionary<string, string> search_params, string[] output_params)
        {
            Connect.Open(mksConnection);
            MySqlCommand cmd = mksConnection.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT " + Parse.OutputParams(output_params) + " FROM " + table + " WHERE " + Parse.SearchParamsHolders(search_params);
            cmd.Prepare();
            Parse.InjectParams(cmd, search_params);
            MySqlDataReader rdr = null;
            rdr = cmd.ExecuteReader();
            string[] read = new string[output_params.Length];
            while (rdr.Read())
            {
                for (int i = 0; i < output_params.Length; i++)
                {
                    if (rdr.IsDBNull(i)) { read[i] = ""; }
                    else { read[i] = rdr.GetString(i); }
                }
            }
            if (rdr != null) { rdr.Close(); }
            Connect.Close(mksConnection);
            return Parse.ReadResults(read, output_params);
        }

        public static bool createVendingTransaction(
            string id, string machineID, 
            DateTime time, string item, 
            double price, string payType, 
            string reference, string siteID
            )
        {
            Dictionary<string, string> vending_params = new Dictionary<string, string>() 
            {
                {"id_number", id},
                {"location", "10"},
                {"qdate", Parse.giveDate(time)},
                {"item", item},
                {"qty", "1"},
                {"amount", Convert.ToString(price)},
                {"payment", payType},
                {"reference", reference},
                {"operator", machineID + siteID},
                {"time", Convert.ToString(time.TimeOfDay)},
                {"school", Parse.readSchool(siteID)},
                {"source", "5"}
            };
            bool resultResponse = insertRecord("transact", vending_params);
            LogDump.Log("CreateVendingTransactionFunction result: " + resultResponse.ToString());
            return resultResponse;
        }
        
        //adds a records to transact with details specifying that it is from a vending machine.
        //this my be changed when the VN functions are changed to add siteID. might have to change how its called in USAT function
        public static bool createVendingTransaction(
            string id, string machineID, 
            DateTime time, string item, 
            double price, string payType, 
            string reference
            )
        {
            Dictionary<string, string> vending_params = new Dictionary<string, string>() 
            {
                {"id_number", id},
                {"location", "10"},
                {"qdate", Parse.giveDate(time)},
                {"item", item},
                {"qty", "1"},
                {"amount", Convert.ToString(price)},
                {"payment", payType},
                {"reference", reference},
                {"operator", machineID},
                {"time", Convert.ToString(time.TimeOfDay)},
                {"school", Parse.readSchool(machineID)},
                {"source", "5"}
            };
            return insertRecord("transact", vending_params);
        }

        //inserts a record into a given table
        public static bool insertRecord(string table, Dictionary<string, string> input_params)
        {
            Connect.Open(mksConnection);
            //test
            LogDump.Log("mysql connection: "+ mksConnection.State.ToString());
            //test
            MySqlCommand cmd = mksConnection.CreateCommand();
            cmd.CommandText = "INSERT INTO " + table + " SET " + Parse.InsertParamsHolders(input_params);
            
            cmd.Prepare();
            Parse.InjectParams(cmd, input_params);
            try
            {
                cmd.ExecuteNonQuery();
                Connect.Close(mksConnection);
                //test
                LogDump.Log("InsertRecord Function result: true");
                //test
                return true;
                
            }
            catch (Exception ex)
            {
                Connect.Close(mksConnection);
                //test
                LogDump.Log("InsertRecord Function result: false");
                //test
                return false;
            }
        }

        //finds multiple rows in a table, returns the sum of a given attribute.
        public static double findSum(string table, Dictionary<string, string> search_params, string sum_field)
        {
            Connect.Open(mksConnection);
            MySqlCommand cmd = mksConnection.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT SUM(" + sum_field + ") FROM " + table + " WHERE " + Parse.SearchParamsHolders(search_params);
            cmd.Prepare();
            Parse.InjectParams(cmd, search_params);
            object result = cmd.ExecuteScalar();
            Connect.Close(mksConnection);
            if (result != DBNull.Value)
            {
                return Convert.ToDouble(result);
            }
            return 0;
        }

        //updates a value, specific case, known uid
        public static bool update(string table, string field, string new_value, string uid)
        {
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"uid", uid}
            };
            return update(table, field, new_value, search_params);
        }

        //updates a value, general case
        public static bool update(string table, string field, string new_value, Dictionary<string, string> search_params)
        {
            string wsString = ConfigurationManager.ConnectionStrings["MKSWSDB"].ConnectionString;
            MySqlConnection wsConnection = new MySqlConnection(wsString);
            Connect.Open(wsConnection);
            MySqlCommand cmd = wsConnection.CreateCommand();
            cmd.CommandText = "UPDATE " + table + " SET " + field + "=" + new_value + " WHERE " + Parse.InsertParamsHolders(search_params);
            cmd.Prepare();
            Parse.InjectParams(cmd, search_params);
            object response = cmd.ExecuteNonQuery();
            Connect.Close(wsConnection);
            return response == null ? false : true;
        }

        //returns multiple rows from a single table
        public static DataTable findMultipleRows(string table, Dictionary<string, string> search_params, string[] output_params)
        {
            Connect.Open(mksConnection);
            MySqlCommand cmd = mksConnection.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT " + Parse.OutputParams(output_params) + " FROM " + table + " WHERE " + Parse.SearchParamsHolders(search_params);
            cmd.Prepare();
            Parse.InjectParams(cmd, search_params);
            MySqlDataReader rdr = null;
            DataTable result = new DataTable();
            foreach (string entry in output_params) { result.Columns.Add(entry, typeof(string)); }
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DataRow newRow = result.Rows.Add();
                foreach (DataColumn col in result.Columns)
                {
                    if (rdr[col.ColumnName] != null) { newRow[col.ColumnName] = rdr[col.ColumnName]; }
                    else { newRow[col.ColumnName] = ""; }
                }
            }
            if (rdr != null) { rdr.Close(); }
            Connect.Close(mksConnection);
            return result;
        }

        //finds multiple rows where the timestamp is less than a week old. specific case, known uid
        public static DataTable findMultipleRowsWithinWeek(string table, string uid, string[] output_params)
        {
            string[] schoolID_params = { "id_number", "school" };
            Dictionary<string, string> search_params = SQL.findRow("student", uid, schoolID_params);
            return findMultipleRowsWithinWeek(table, search_params, output_params);
        }

        //finds multiple rows where the timestamp is less than a week old. general case.
        public static DataTable findMultipleRowsWithinWeek(string table, Dictionary<string, string> search_params, string[] output_params)
        {
            Connect.Open(mksConnection);
            MySqlCommand cmd = mksConnection.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT " + Parse.OutputParams(output_params) + " FROM " + table + " WHERE " + Parse.SearchParamsHolders(search_params);
            cmd.Prepare();
            Parse.InjectParams(cmd, search_params);
            cmd.CommandText += " AND DATEDIFF(CURRENT_TIMESTAMP,orderdate) <= 7";
            MySqlDataReader rdr = null;
            DataTable result = new DataTable();
            foreach (string entry in output_params) { result.Columns.Add(entry, typeof(string)); }
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DataRow newRow = result.Rows.Add();
                foreach (DataColumn col in result.Columns)
                {
                    if (rdr[col.ColumnName] != null) { newRow[col.ColumnName] = rdr[col.ColumnName]; }
                    else { newRow[col.ColumnName] = ""; }
                }
            }
            if (rdr != null) { rdr.Close(); }
            Connect.Close(mksConnection);
            return result;
        }

        //find a row that has the maximum value of a given attribute.
        public static Dictionary<string, string> findMaxRow(string table, Dictionary<string, string> search_params, string[] output_params, string max_field)
        {
            Connect.Open(mksConnection);
            MySqlCommand cmd = mksConnection.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT " + Parse.OutputParams(output_params) + " FROM " + table + " WHERE " + Parse.SearchParamsHolders(search_params)
                + " order by " + max_field + " desc limit 1";
            cmd.Prepare();
            Parse.InjectParams(cmd, search_params);
            MySqlDataReader rdr = null;
            rdr = cmd.ExecuteReader();
            string[] read = new string[output_params.Length];
            while (rdr.Read())
            {
                for (int i = 0; i < output_params.Length; i++)
                {
                    if (rdr.IsDBNull(i)) { read[i] = ""; }
                    else { read[i] = rdr.GetString(i); }
                }
            }
            if (rdr != null) { rdr.Close(); }
            Connect.Close(mksConnection);
            return Parse.ReadResults(read, output_params);
        }

        //find a student by a groupID number.
        public static DataTable findByGroup(string groupID, string school)
        {
            Connect.Open(mksConnection);
            MySqlCommand cmd = mksConnection.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT s.id_number, s.student, s.last_name, s.uid FROM groupid g, student s WHERE g.GROUPID = @group "
            + "AND (s.id_number = g.id_number) AND s.school = @school AND g.school= s.school";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@group", groupID);
            cmd.Parameters.AddWithValue("@school", school);
            MySqlDataReader rdr = null;
            DataTable result = new DataTable();
            foreach (string entry in new string[] { "id_number", "student", "last_name", "uid" }) { result.Columns.Add(entry, typeof(string)); }
            rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DataRow newRow = result.Rows.Add();
                foreach (DataColumn col in result.Columns)
                {
                    if (rdr[col.ColumnName] != null) { newRow[col.ColumnName] = rdr[col.ColumnName]; }
                    else { newRow[col.ColumnName] = ""; }
                }
            }
            if (rdr != null) { rdr.Close(); }
            Connect.Close(mksConnection);
            return result;

        }
    }
}
