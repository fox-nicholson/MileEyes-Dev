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

            dynamic sg = new SendGridAPIClient(apiKey);

            var from = new Email("noreply@mileeyes.com");
            var subject = message.Subject;
            var to = new Email(message.Destination);
            var content = new Content("text/html", message.Body);
            var mail = new Mail(from, subject, to, content);

            dynamic response = await sg.client.mail.send.post(requestBody: mail.Get());
        }
    }
}