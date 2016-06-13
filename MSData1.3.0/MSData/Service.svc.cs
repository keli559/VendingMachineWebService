using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MSData
{
    
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Service
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string AdjustBalance(string Uid, string Adjustment)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string current = SQL.findScalar("student", Uid, "present");
            string target = Convert.ToString(Convert.ToDouble(current) + Convert.ToDouble(Adjustment));
            return SQL.update("student", "present", target, Uid) ? "OK" : "NOK";
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string AuthPortable(string Uid, string Serial)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string[] output_params = { "serial", "server", "db", "port", "user", "school", "scan_barcode", "check_balance", "MSSQL", "PHPpath" };
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"uid", Uid},
                {"serial", Serial}
            };
            Dictionary<string, string> authOutputs = SQL.findRow("portable.authenticate", search_params, output_params);
            if (authOutputs["server"] == null) { return "Not Authenticated"; }
            string result = "";
            result += @"""server"":""" + authOutputs["server"] + @""",";
            result += @"""db"":""" + authOutputs["db"] + @""",";
            result += @"""port"":" + authOutputs["port"] + @",";
            result += @"""user"":""" + authOutputs["user"] + @""",";
            result += @"""school"":""" + authOutputs["school"] + @""",";
            result += @"""scan_barcode"":" + authOutputs["scan_barcode"] + @",";
            result += @"""check_balance"":" + authOutputs["check_balance"] + @",";
            result += @"""MSSQL"":" + authOutputs["MSSQL"] + @",";
            result += @"""PHPpath"":""" + authOutputs["PHPpath"] + @"""";
            return result;
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string Balance(string Uid)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            if (Uid == null) { return "0"; }
            string[] output_params = { "present", "school", "id_number" };
            Dictionary<string, string> studOutput = SQL.findRow("student", Uid, output_params);
            if (studOutput["school"] == "NMH" || studOutput["school"] == "CCS")
            {
                return studOutput["present"];
            }
            double bal = Calc.balance(studOutput["id_number"], studOutput["school"]);

            if (studOutput["id_number"] == null) { return "0"; }
            Dictionary<string, string> insert_params = new Dictionary<string, string>()
            {
                {"id_number", studOutput["id_number"]},
                {"school", studOutput["school"]},
                {"balance", bal.ToString()}
            };

            SQL.insertRecord("bal_inquiry", insert_params);
            return bal.ToString();
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string GetAreas(string Uid, string School, string Version)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string foundSchool = SQL.findScalar("student", Uid, "school");
            if (foundSchool == "" || foundSchool != School) { return "NOK"; }
            string[] output_params = { "area", "location" };
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"school", School}
            };
            DataTable output = SQL.findMultipleRows("portable.areas", search_params, output_params);
            string result = @"<plist version=""1.0""><dict>" + Environment.NewLine;
            result += "<key>1</key>" + Environment.NewLine;
            result += "<dict>" + Environment.NewLine;
            foreach (DataRow row in output.Rows)
            {
                result += "<key>" + row["area"] + "</key>" + Environment.NewLine;
                result += "<string>" + row["location"] + "</string>" + Environment.NewLine;
            }
            result += "</dict>" + Environment.NewLine;
            result += "</dict></plist>";
            return result;
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string GetDepartment(string Uid, string School, string Version, string Location)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string foundSchool = SQL.findScalar("student", Uid, "school");
            if (foundSchool == "" || foundSchool != School) { return "NOK"; }
            string[] output_params = { "dept_code", "department" };
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"school", School},
                {"location", Location}
            };
            DataTable output = SQL.findMultipleRows("portable.departments", search_params, output_params);
            string result = @"<plist version=""1.0""><dict>" + Environment.NewLine
                + "<key>1</key>" + Environment.NewLine
                + "<dict>" + Environment.NewLine;
            foreach (DataRow row in output.Rows)
            {
                result += "<key>" + row["department"] + "</key>" + Environment.NewLine
                    + "<string>" + row["dept_code"] + "</string>" + Environment.NewLine;
            }

            result += "</dict>" + Environment.NewLine
                + "</dict></plist>";
            return result;
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string GetItems(string Uid, string School, string Version, string Location)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string foundSchool = SQL.findScalar("student", Uid, "school");
            if (foundSchool == "" || foundSchool != School) { return "NOK"; }
            string[] output_params = { "item", "retail", "plu", "stock", "dept_code", "taxable", "taxrate", "requirefunds", "purchase_limit", "barcode", "id" };
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"school", School},
                {"location", Location}
            };
            DataTable output = SQL.findMultipleRows("portable.stock", search_params, output_params);

            output.DefaultView.Sort = "dept_code";
            output = output.DefaultView.ToTable();
            DataRow newRow = output.NewRow();
            newRow["dept_code"] = "99999999999999999999999NOTADEPTzzzzzzz";
            output.Rows.Add(newRow);
            string dep = "0";
            string itemArray, items, retail, plu, stock, limit, taxable, taxrate, require;
            itemArray = items = retail = plu = stock = limit = taxable = taxrate = require = "";
            for (int i = 0; i < output.Rows.Count; i++)
            {
                if (output.Rows[i]["dept_code"].ToString() != dep)
                {
                    if (items != "")
                    {
                        itemArray += "<key>" + dep + "</key>" + Environment.NewLine
                         + "<dict>" + Environment.NewLine
                         + "<key>item</key>" + Environment.NewLine
                         + "<array>" + items + "</array>" + Environment.NewLine
                         + "<key>retail</key>" + Environment.NewLine
                         + "<array>" + retail + "</array>" + Environment.NewLine
                         + "<key>plu</key>" + Environment.NewLine
                         + "<array>" + plu + "</array>" + Environment.NewLine
                         + "<key>stock</key>" + Environment.NewLine
                         + "<array>" + stock + "</array>" + Environment.NewLine
                         + "<key>limit</key>" + Environment.NewLine
                         + "<array>" + limit + "</array>" + Environment.NewLine
                         + "<key>taxable</key>" + Environment.NewLine
                         + "<array>" + taxable + "</array>" + Environment.NewLine
                         + "<key>taxrate</key>" + Environment.NewLine
                         + "<array>" + taxrate + "</array>" + Environment.NewLine
                         + "<key>requirefunds</key>" + Environment.NewLine
                         + "<array>" + require + "</array>" + Environment.NewLine
                         + "</dict>" + Environment.NewLine;
                    }
                    items = retail = plu = stock = limit = taxable = taxrate = require = "";
                }
                items += "<string>" + output.Rows[i]["item"] + "</string>";
                retail += "<string>" + Parse.truncNumb(output.Rows[i]["retail"].ToString()) + "</string>";
                plu += "<string>" + output.Rows[i]["plu"] + "</string>";
                stock += "<string>" + output.Rows[i]["stock"] + "</string>";
                limit += "<string>" + output.Rows[i]["purchase_limit"] + "</string>";
                taxable += "<string>" + output.Rows[i]["taxable"] + "</string>";
                taxrate += "<string>" + Parse.truncNumb(output.Rows[i]["taxrate"].ToString()) + "</string>";
                require += "<string>" + output.Rows[i]["requirefunds"] + "</string>";
                dep = output.Rows[i]["dept_code"].ToString();
            }
            string result = @"<plist version=""1.0""><dict>" + Environment.NewLine
                + itemArray
                + "</dict></plist>";
            return result;
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string GetOrderText(string Uid)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string school = SQL.findScalar("student", Uid, "school");
            if (school == "") { return "NOK"; }
            if (school == "Chadwick") { return "Each order includes a bottle of water and a piece of fruit.  All orders are delivered to Roessler Rotunda by 11 am"; }
            return "";
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string GetOrders(string Uid)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string school = SQL.findScalar("student", Uid, "school");
            if (school == "") { return "NOK"; }

            string[] output_params = { "status", "orderid", "UNIX_TIMESTAMP(orderdate)" };
            DataTable output = SQL.findMultipleRowsWithinWeek("portable.orders", Uid, output_params);

            string result = @"<plist version=""1.0""><dict>" + Environment.NewLine;

            int x = 1;
            foreach (DataRow row in output.Rows)
            {
                result += "<key>" + x + "</key><dict>";
                result += "<key>OrderID</key>";
                result += "<string>" + row["orderid"] + "</string>";
                result += "<key>OrderDate</key>";
                result += "<string>" + row["UNIX_TIMESTAMP(orderdate)"] + "</string>";
                result += "<key>OrderStatus</key>";
                result += "<string>" + row["status"] + "</string></dict>";
                x++;
            }
            if (output.Rows.Count > 0) { result += Environment.NewLine; }
            result += "</dict></plist>";
            return result;
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string LastTransfer(string Uid)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string[] output_params = { "school", "id_number" };
            Dictionary<string, string> outputs = SQL.findRow("student", Uid, output_params);
            if (outputs["school"] == null) { return "Not found"; }
            string[] secondary_params = { "qDate", "sequence", "id_number" };
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"id_number", outputs["id_number"]},
                {"type", "Dep"},
                {"method", "0"},
                {"school", outputs["school"]}
            };
            Dictionary<string, string> transfer = SQL.findMaxRow("transfer", search_params, secondary_params, "sequence");
            string result = Convert.ToString(Calc.balance(outputs["id_number"], outputs["school"]));
            if (transfer["sequence"] == null) { return result + "," + Parse.truncTime(DateTime.Now) + ",0," + "No web deposits," + outputs["id_number"]; }
            search_params.Add("sequence", transfer["sequence"]);
            search_params.Add("qDate", Parse.giveDate(Convert.ToDateTime(transfer["qDate"])));
            double amount = SQL.findSum("transfer", search_params, "amount");
            return result + "," + Parse.truncTime(transfer["qDate"]) + "," + Convert.ToString(amount) + "," + transfer["sequence"] + "," + transfer["id_number"];
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string Login(string User, string Password, string Version)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string[] output_params = { "id", "school", "forceverifyemail", "enabled" };
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"UserLogin", User},
                {"password", Password}
            };
            Dictionary<string, string> outputs = SQL.findRow("security", search_params, output_params);
            if (outputs["school"] == null || outputs["enabled"] == "0") { return "Not found"; }
            string[] secondary_params = { "student", "last_name", "uid" };
            DataTable students = SQL.findByGroup(outputs["id"], outputs["school"]);
            DataRow firstRow = students.Rows[0];
            return firstRow["uid"] + @",""" + firstRow["student"] + @""",""" + firstRow["last_name"] + @"""," + outputs["school"];
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string LoginArray(string User, string Password, string Version)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string[] output_params = { "id", "school", "forceverifyemail", "enabled" };
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"UserLogin", User},
                {"password", Password}
            };
            Dictionary<string, string> outputs = SQL.findRow("security", search_params, output_params);
            if (outputs["school"] == null || outputs["enabled"] == "0") { return "Not found"; }
            string[] secondary_params = { "student", "last_name", "uid", "school" };
            DataTable students = SQL.findByGroup(outputs["id"], outputs["school"]);
            string parentStr = "";
            if (students.Rows.Count == 0) { return "Not found"; }
            if (students.Rows.Count > 1)
            {
                string[] parent_output = { "student", "last_name", "school" };
                Dictionary<string, string> parent_params = new Dictionary<string, string>
                {
                    {"id_number", outputs["id"]},
                    {"accnt_type", "WG"}
                };
                Dictionary<string, string> parent = SQL.findRow("student", parent_params, parent_output);
                parentStr = "<key>0</key>" + Environment.NewLine
                    + "<array>" + Environment.NewLine
                    + "<string>0</string>" + Environment.NewLine
                    + "<string>" + parent["student"] + "</string>" + Environment.NewLine
                    + "<string>" + parent["last_name"] + "</string>" + Environment.NewLine
                    + "<string>" + parent["school"] + "</string>" + Environment.NewLine
                    + "</array>";
            }

            string result = @"<plist version=""1.0""><dict>" + Environment.NewLine;
            if (parentStr != "") { result += parentStr + Environment.NewLine; }
            result += "<key>1</key>" + Environment.NewLine
            + "<dict>" + Environment.NewLine;
            for (int i = 0; i < students.Rows.Count; i++)
            {
                result += "<key>" + (i + 1).ToString() + "</key>" + Environment.NewLine
                    + "<array>" + Environment.NewLine
                    + "<string>" + students.Rows[i]["uid"] + "</string>" + Environment.NewLine
                    + "<string>" + students.Rows[i]["student"] + "</string>" + Environment.NewLine
                    + "<string>" + students.Rows[i]["last_name"] + "</string>" + Environment.NewLine
                    + "<string>" + outputs["school"] + "</string>" + Environment.NewLine
                    + "</array>" + Environment.NewLine;
            }
            if (parentStr != "") { result += "</dict>" + Environment.NewLine; }
            result += "</dict></plist>";
            return result;
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string PortableArray(string Uid, string Serial)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string[] output_params = { "uid", "scan_barcode", "check_balance" };
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"serial", Serial}
            };
            Dictionary<string, string> outputs = SQL.findRow("portable.authenticate", search_params, output_params);
            if (outputs["uid"] == null || outputs["uid"] != Uid) { return "Not Authenticated"; }
            return null;
        }
        
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string RcvTran(string TranData) 
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            //TranData = "<id_number>2597</id_number><school>Valhalla</school><name>Holiday Ball</name><plus>158</plus><quantity>1</quantity><retails>10</retails><departments>0</departments><total>10</total><glcode>500</glcode><reference>I 282</reference><qdate>2015-05-18</qdate><time>15:35:35</time><first>ERROR</first><last>ERROR</last><location>5</location><source>2</source><type>Sale</type><payment>D</payment><tax_amount>0</tax_amount><operator>TestDev</operator><process_on_sync>1</process_on_sync><orderDate>1431963336.206344</orderDate><deviceID>4ED1BBB0-E143-45C1-AC49-11AD3A415028</deviceID><iosVersion>7.1.2</iosVersion>";
            if (TranData == null) { return "1"; }
            Regex.Replace(TranData, "\t|\n|\r", "");
            Dictionary<string, string> input_params = new Dictionary<string, string>() 
            {
            {"rawdata", TranData},
            {"length", Convert.ToString(TranData.Length)}
            };
            if (!SQL.insertRecord("portable.rawtran", input_params)) { return "1"; }
            Dictionary<string, string> trans = Parse.stringToDictionary(TranData);

            string type ="";
            bool lOrder, lcc, lReceipt, lAccount;
            lOrder = lcc = lReceipt = lAccount = false;

            if (trans.ContainsKey("uid")) 
            {
                lOrder = true;
                type = "order";
            }
            
            if (trans.ContainsKey("cc"))
            {
                lcc = true;
                type = "cc";
            }
            
            if (trans.ContainsKey("email"))
            {
                lReceipt = true;
                type = "receipt";
            }
            
            if (trans.ContainsKey("id_number"))
            {
                lAccount = true;
                type = "trans";
            }
            
            if (!trans.ContainsKey("deviceID") || !trans.ContainsKey("school")) { return "1"; }
            Dictionary<string,string> device_params = new Dictionary<string,string>()
            {
                {"deviceid", trans["deviceID"]},
                {"school", trans["school"]},
                {"trantype", type}
            };
            if (!SQL.insertRecord("portable.devices", device_params)) { return "1"; }

            string from = "AccountSetup@MyKidsSpending.com";
            string recipient = "develop@odin-inc.com";
            
            string subject;
            if (lcc) { subject = "Webservice cc transaction RcvTran received " + trans["school"]; }
            else if (lOrder) { subject = "Webservice order RcvTran received " + trans["school"]; }
            else if (lAccount) { subject = "Webservice id_number RcvTran received " + trans["school"]; }
            else { subject = "Webservice transaction RcvTran received " + trans["school"]; }

            Mailer.send(from, recipient, null, subject, TranData);

            string[] student_params = { "id_number", "accnt_type" };
            Dictionary<string, string> student = new Dictionary<string, string>() { {"id_number", "0"} };
            if (trans.ContainsKey("uid")) { student = SQL.findRow("student", trans["uid"], student_params); }

            string name = ""; if (trans.ContainsKey("name")) { name = Parse.shave(trans["name"]); }
            string items = ""; if (trans.ContainsKey("name")) { items = Parse.shave(trans["name"]); }
            string plus = ""; if (trans.ContainsKey("plus")) { plus = Parse.shave(trans["plus"]); }
            string quantities = ""; if (trans.ContainsKey("quantity")) { quantities = Parse.shave(trans["quantity"]); }
            string retails = ""; if (trans.ContainsKey("retails")) { retails = Parse.shave(trans["retails"]); }
            string tax = "0"; if (trans.ContainsKey("tax")) { tax = Parse.shave(trans["tax"]); }
            string glcodes = ""; if (trans.ContainsKey("glcode")) { glcodes = Parse.shave(trans["glcode"]); }
            string departments = ""; if (trans.ContainsKey("department")) { glcodes = Parse.shave(trans["department"]); }

            if (lcc && !lOrder)
            {
                Dictionary<string, string> webtran_params = new Dictionary<string, string>() 
                {
                    {"last4", trans["cc"]},
                    {"first", trans["first"]},
                    {"last", trans["last"]},
                    {"trandate", trans["qdate"]},
                    {"amount", trans["total"]},
                    {"reference", trans["reference"]},
                    {"type", trans["type"]},
                    {"transactionid", trans["transactionid"]},
                    {"approval", trans["approval"]},
                    {"responsetext", trans["responsetext"]},
                    {"school", trans["school"]}
                };
                if (!SQL.insertRecord("portable.webtran", webtran_params)) { return "1"; }
            }
            string receiptText = "";
            if (lReceipt)
            {
                if (trans.ContainsKey("header1")) { receiptText += @"<div><font size=""4""><b>" + trans["header1"] + "<br>"; }
                else { receiptText += @"<div><font size=""4""><b>"; }
                if (trans.ContainsKey("header2")) { receiptText += trans["header2"] + "<br>"; }
                if (trans.ContainsKey("header3")) { receiptText += trans["header3"] + "<br>"; }
                if (trans.ContainsKey("header4")) { receiptText += trans["header4"] + "<br>"; }
                if (trans.ContainsKey("header5")) { receiptText += trans["header5"] + "<br><br></b></font></div>"; }
                else { receiptText += "<br></b></font></div>"; }

                receiptText += "Name: " + trans["first"] + " "+ trans["last"] + "<br>";
                receiptText += "Date: " + trans["qdate"] + "<br>";
                receiptText += "Time: " + trans["time"]  + "<br><br>";
            }

            int x = Regex.Split(quantities, ",").Length;
            for (int i = 0; i < x; i++) 
            {
                string item = Regex.Match(items, ".+?(?=,)").Value;
                if (item == "") { item = items; }
                items = items.Remove(0, item.Length);
                if (items != "") { items = items.Remove(0, 2); }

                string plu = Regex.Match(plus, ".+?(?=,)").Value;
                if (plu == "") { plu = plus; }
                plus = plus.Remove(0, plu.Length);
                if (plus != "") { plus = plus.Remove(0, 2); }

                string quantity = Regex.Match(quantities, ".+?(?=,)").Value;
                if (quantity == "") { quantity = quantities; }
                quantities = quantities.Remove(0, quantity.Length);
                if (quantities != "") { quantities = quantities.Remove(0, 2); }

                string retail = Regex.Match(retails, ".+?(?=,)").Value;
                if (retail == "") { retail = retails; }
                retails = retails.Remove(0, retail.Length);
                if (retails != "") { retails = retails.Remove(0, 2); }

                string department = Regex.Match(departments, ".+?(?=,)").Value;
                if (department == "") { department = departments; }
                departments = departments.Remove(0, department.Length);
                if (departments != "") { departments = departments.Remove(0, 2); }

                string glcode = Regex.Match(glcodes, ".+?(?=,)").Value;
                if (glcode == "") { glcode = glcodes; }
                glcodes = glcodes.Remove(0, glcode.Length);
                if (glcodes != "") { glcodes = glcodes.Remove(0, 2); }

                Dictionary<string, string> indv_transact_params = new Dictionary<string, string>()
                {
                    {"id_number", student["id_number"]},
                    {"qDate", trans.ContainsKey("qDate") ? trans["qdate"] : ""},
                    {"time", trans.ContainsKey("time") ? trans["time"] : ""},
                    {"item", item},
                    {"plu", plu},
                    {"qty", quantity},
                    {"tax_amount", tax},
                    {"amount", retail},
                    {"reference", trans.ContainsKey("reference") ? trans["reference"] : ""},
                    {"school", trans.ContainsKey("school") ? trans["school"] : ""},
                    {"source", trans.ContainsKey("source") ? trans["source"] : ""},
                    {"payment", trans.ContainsKey("payment") ? trans["payment"] : ""},
                    {"location", trans.ContainsKey("location") ? trans["location"] : ""},
                    {"dept_code", department},
                    {"glcode", glcode},
                    {"operator", trans.ContainsKey("operator") ? trans["operator"] : ""}
                };

                if (lOrder)
                {
                    Dictionary<string, string> indv_order_params = new Dictionary<string, string>()
                    {
                        {"id_number", student["id_number"]},
                        {"item", item},
                        {"plu", plu},
                        {"quantity", quantity},
                        {"amount", retail},
                        {"tax", tax},
                        {"total", Convert.ToString(Convert.ToInt32(quantity) * Convert.ToDouble(retail) + Convert.ToDouble(tax))},
                        {"comment", trans["comment"]},
                        {"school", trans["school"]},
                        {"orderid", trans["orderID"]},
                        {"orderdate", trans["orderDate"]}
                    };
                    if (!SQL.insertRecord("portables.orders", indv_order_params)) { return "1"; }
                }
                else if (lcc)
                {
                    if (lReceipt)
                    {
                        receiptText += @"<div><span style=3D""font-family:monospace,monospace"">"+ item + ", Quantity: " + quantity + ", Price: " + retail + "<br></span></div>";
                    }
                    else 
                    {
                        if (!SQL.insertRecord("transact", indv_transact_params)) { return "1"; }
                    }
                }
                else if (lAccount)
                {
                    if (!SQL.insertRecord("transact", indv_transact_params)) { return "1"; };
                }
            }
            if (lReceipt)
            {
                receiptText += trans.ContainsKey("tax_amount") ? trans["tax_amount"] : "0";
                receiptText += "<br></span></div>"
                    + @"<div><span style=3D""font-family:monospace,monospace"">" + "Total: " + trans["total"] + "<br><br></span></div>"
                    + @"Card Number: XXXXXXXXXXXX" + trans["cc"] + "<br>"
                    + @"Transaction ID: " + trans["transactionid"] + "<br>"
                    + @"Transaction Type: " + trans["type"] + "<br>"
                    + @"Reference: " + trans["reference"] + "<br>"
                    + @"Approval: "+ trans["approval"] + "<br>";

                subject = trans["school"] + " Transaction Receipt";
                string header = "Content-Type: text/html; charset=UTF-8" + Environment.NewLine + "Content-Transfer-Encoding: quoted-printable";
                recipient += ", " + trans["email"];
                Mailer.send(from, recipient, null, subject, receiptText);
            }          
            return "0";
        }

        //original port was 465. not sure if important.
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Xml)]
        public string SendPassword(string User)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            string[] output_params = { "password", "school", "id", "uid" };
            Dictionary<string, string> search_params = new Dictionary<string, string>()
            {
                {"UserLogin", User}
            };
            Dictionary<string, string> login = SQL.findRow("security", search_params, output_params);
            DataTable person = SQL.findByGroup(login["id"], login["school"]);
            string from = "AccountSetup@MyKidsSpending.com";
            string bcc = "Statements@MyKidsSpending.com";
            string subject = "MyKidsSpending login " + login["school"];
            string body = "Dear: " + person.Rows[0]["student"] + " " + person.Rows[0]["last_name"] + Environment.NewLine
                + "Your password  is " + login["password"] + Environment.NewLine
                + "https://www.mykidsspending.com/" + login["school"] + ".aspx" + Environment.NewLine
                + "PLEASE DO NOT REPLY TO THIS E-MAIL. "
                + "THIS E-MAIL ADDRESS IS USED BY MYKIDSSPENDING AUTOMATED SYSTEMS AND IS NOT MONITORED. "
                + "IF YOU WISH TO CONTACT US, PLEASE SEND AN E-MAIL TO SUPPORT@MYKIDSSPENDING.COM" + Environment.NewLine;
            if (Mailer.send(from, User, bcc, subject, body)) { return "Your password has been sent."; }
            else { return "Email server error"; }
            //string userName = ConfigurationManager.ConnectionStrings["SMTPUserName"].ConnectionString;
            //string password = ConfigurationManager.ConnectionStrings["SMTPPassword"].ConnectionString;
            //SmtpClient client = new SmtpClient("email-smtp.us-east-1.amazonaws.com", 587);
            //client.Credentials = new System.Net.NetworkCredential(userName, password);
            //client.EnableSsl = true;
            //try
            //{
            //client.Send(message);
            //return "Your password has been sent.";
            //}
            //catch (Exception ex)
            //{
            //return "Email server error";
            //}
        }
    }
}
