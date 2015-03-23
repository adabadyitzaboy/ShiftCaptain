using ShiftCaptain.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace ShiftCaptain.Infrastructure
{
    public static class EmailManager
    {
        private static bool GetConfig(String setting)
        {
            bool value;
            if (bool.TryParse(ConfigurationManager.AppSettings[setting], out value))
            {
                return value;
            }
            return false;
        }
        public static bool SendMail(EmailTemplate template, ref String ErrorMessage)
        {
            try
            {
                if (template.From.IndexOf("@emails.com") != -1 || template.To.IndexOf("@emails.com") != -1)
                {
                    return true;
                }
                var client = new SmtpClient(ConfigurationManager.AppSettings["SmtpClient"]);
                var message = new MailMessage(template.From, template.To);
                if(!String.IsNullOrEmpty(template.Cc)){
                    foreach (var address in template.Cc.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!String.IsNullOrEmpty(address))
                        {
                            message.CC.Add(address.Trim());
                        }
                    }
                }
                if (!String.IsNullOrEmpty(template.Bcc))
                {
                    foreach (var address in template.Bcc.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!String.IsNullOrEmpty(address))
                        {
                            message.Bcc.Add(address.Trim());
                        }
                    }
                }
                message.Subject = template.Subject;
                message.IsBodyHtml = true;
                message.Body = template.Body;

                if (GetConfig("EmailUseCredentials"))
                {
                    client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["EmailCredentialsUserName"], "EmailCredentialsPassword");
                }
                client.EnableSsl = GetConfig("EnableSSL");
                client.Send(message);
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message + " " + e.InnerException;
                return false;
            }
            return true;
        }
    }
}