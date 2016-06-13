using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace MSData
{
    public static class Connect
    {
        //try to conenct to SQL with the given string and credentials
        public static bool Open(MySqlConnection connection)
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException exception)
            {
                return false;
            }
        }

        //close the given SQL connection
        public static bool Close(MySqlConnection connection)
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException exception)
            {
                return false;
            }
        }
    }
}