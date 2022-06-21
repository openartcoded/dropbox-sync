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
                throw new NullValueException("There is no value in environment variable named MAIL_SENDER_EMAIL!");

            string senderPassword = Environment.GetEnvironmentVariable("MAIL_SENDER_PASSWORD") ??
                throw new NullValueException("There is no value for environment variable named MAIL_SENDER_PASSWORD!");

            string host = Environment.GetEnvironmentVariable("MAIL_SENDER_SERVER") ??
                throw new NullValueException("There is no value in environment variable named MAIL_SENDER_SERVER!");

            if (!int.TryParse(Environment.GetEnvironmentVariable("MAIL_SENDER_PORT"), out int port))
                throw new FormatException("MAIL_SENDER_PORT couldn't be parsed to an int value!");

            if (!bool.TryParse(Environment.GetEnvironmentVariable("MAIL_SENDER_SSL_ENABLE"), out bool enableSsl))
                throw new FormatException("MAIL_SENDER_SSL_ENABLE couldn't be parsed to bool value!");

            MailMessage message = new MailMessage(senderEmail, receiverEmail);
            message.Subject = "Dropbox Synchronisation : Connection to the backoffice broker lost";
            message.Body =
            @"The connection to the backoffice's broker is lost. Events wont be proceeded, please wait until the service 
            reconnect to the broker.
            
            Thank you";

            SmtpClient client = new SmtpClient(host, port);
            client.EnableSsl = enableSsl;
            client.Credentials = new NetworkCredential(senderEmail, senderPassword);

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