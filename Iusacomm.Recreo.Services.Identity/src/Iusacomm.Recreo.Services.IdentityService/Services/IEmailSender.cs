using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iusacomm.Recreo.Services.IdentityService.Services
{
    public interface IEmailSender
    {
        /// <summary>
        /// Sends the email asynchronous.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="htmlMessage">The HTML message.</param>
        /// <param name="textMessage">The text message.</param>
        /// <returns></returns>
        Task SendEmailAsync(string email, string subject, string htmlMessage, string textMessage);
    }
}
