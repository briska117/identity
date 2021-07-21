using Iusacomm.Recreo.Services.IdentityService.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Iusacomm.Recreo.Services.IdentityService.Services
{
        public class EmailSender : IEmailSender
        {
            private MessagingSettings _messagingSettings;

            public EmailSender(IOptions<MessagingSettings> messagingSettings)
            {
                _messagingSettings = messagingSettings.Value;
            }

            public async Task SendEmailAsync(string email, string subject, string htmlMessage, string textMessage)
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_messagingSettings.SendGridFromAddress, _messagingSettings.SendGridFromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);
                //mailMessage.Bcc.Add("carlos.valencia@iusacomm.com");

                var smtpClient = new SmtpClient
                {
                    Host = "smtp.office365.com",
                    Port = 587,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_messagingSettings.SendGridUserName, _messagingSettings.SendGridPassword)
                };

                smtpClient.Send(mailMessage);

                await Task.FromResult(0);
            }
        }
}
