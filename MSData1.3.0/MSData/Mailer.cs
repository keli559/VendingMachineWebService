using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Configuration;

namespace MSData
{
    public class Mailer
    {
        //send an email
        public static bool send(string sender, string recipient, string bcc, string subject, string body)
        {
            MailMessage message = new MailMessage();
            message.IsBodyHtml = true;
            message.From = new MailAddress(sender);
            message.To.Add(recipient);
            if (bcc != null && bcc != "") { message.Bcc.Add(bcc); }
            message.ReplyToList.Add(sender);
            message.Subject = subject;
            message.Body = body;
            string userName = ConfigurationManager.ConnectionStrings["SMTPUserName"].ConnectionString;
            string password = ConfigurationManager.ConnectionStrings["SMTPPassword"].ConnectionString;
            SmtpClient client = new SmtpClient("email-smtp.us-east-1.amazonaws.com", 587);
            client.Credentials = new System.Net.NetworkCredential(userName, password);
            client.EnableSsl = true;
            try
            {
                client.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}