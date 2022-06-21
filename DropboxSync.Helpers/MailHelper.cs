using System.Net;
using System.Net.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DropboxSync.Helpers
{
    public static class MailHelper
    {
        public static bool SendBrokerConnectionLostEmail()
        {
            string receiverEmail = Environment.GetEnvironmentVariable("MAIL_RECEIVER_ADDRESS") ??
                throw new NullValueException("There is no value in environment variable named MAIL_RECEIVER_ADDRESS!");

            string senderEmail = Environment.GetEnvironmentVariable("MAIL_SENDER_EMAIL") ??
                "noreply@somehost.com";
            string senderPassword = Environment.GetEnvironmentVariable("MAIL_SENDER_PASSWORD") ??
                "noreply";
            string host = Environment.GetEnvironmentVariable("MAIL_SENDER_SERVER") ??
                "greenmail.somehost.org";

            if (!int.TryParse(Environment.GetEnvironmentVariable("MAIL_SENDER_PORT"), out int port))
                port = 25;

            if (!bool.TryParse(Environment.GetEnvironmentVariable("MAIL_SENDER_SSL_ENABLE"), out bool enableSsl))
                enableSsl = true;

            MailMessage message = new MailMessage(senderEmail, receiverEmail);
            message.Subject = "Dropbox Synchronisation : Connection to the backoffice broker lost";
            message.Body = @"The connection to the backoffice's broker is lost. Events wont be proceeded, please wait until the service 
            reconnect to the broker.";

            SmtpClient client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail, senderPassword)
            };

            try
            {
                client.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine(ex.InnerException?.Message);
                return false;
            }
        }
    }
}