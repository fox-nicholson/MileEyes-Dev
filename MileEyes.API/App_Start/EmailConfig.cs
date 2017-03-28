using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MileEyes.API.App_Start
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            return configSendGridasync(message);
        }


        private async Task configSendGridasync(IdentityMessage message)
        {
            var apiKey = ConfigurationManager.AppSettings["SendGridKey"];

            var client = new SendGridClient(apiKey);

            var from = new EmailAddress("noreply@mileeyes.com");
            var to = new EmailAddress(message.Destination);

            var msg = MailHelper.CreateSingleEmail(from, to, message.Subject, "", message.Body);
            var response = await client.SendEmailAsync(msg);
        }

        public static void SendEmail(string email, string subject, string body)
        {
            var apiKey = ConfigurationManager.AppSettings["SendGridKey"];

            var client = new SendGridClient(apiKey);

            var from = new EmailAddress("noreply@mileeyes.com");
            var to = new EmailAddress(email);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", body);
            var response = client.SendEmailAsync(msg);
        }        
    }
}