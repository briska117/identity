using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iusacomm.Recreo.Services.IdentityService.Configuration
{
    public class MessagingSettings
    {
        /// <summary>
        /// Gets or sets the name of the send grid user.
        /// </summary>
        /// <value>
        /// The name of the send grid user.
        /// </value>
        public string SendGridUserName { get; set; }

        /// <summary>
        /// Gets or sets the send grid password.
        /// </summary>
        /// <value>
        /// The send grid password.
        /// </value>
        public string SendGridPassword { get; set; }

        /// <summary>
        /// Gets or sets the send grid from address.
        /// </summary>
        /// <value>
        /// The send grid from address.
        /// </value>
        public string SendGridFromAddress { get; set; }

        /// <summary>
        /// Gets or sets the name of the send grid from.
        /// </summary>
        /// <value>
        /// The name of the send grid from.
        /// </value>
        public string SendGridFromName { get; set; }

        /// <summary>
        /// Gets or sets the send grid API key.
        /// </summary>
        /// <value>
        /// The send grid API key.
        /// </value>
        public string SendGridApiKey { get; set; }
    }
}
