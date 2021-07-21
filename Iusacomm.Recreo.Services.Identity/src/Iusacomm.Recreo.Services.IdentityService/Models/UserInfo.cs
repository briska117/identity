using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iusacomm.Recreo.Services.IdentityService.Models
{
    public class UserInfo
    {
        /// <summary>Gets or sets the email.</summary>
        /// <value>The email.</value>
        public string Email { get; set; }

        /// <summary>Gets or sets the role.</summary>
        /// <value>The role.</value>
        public string Role { get; set; }

        /// <summary>Gets or sets a value indicating whether [email confirmed].</summary>
        /// <value>
        ///   <c>true</c> if [email confirmed]; otherwise, <c>false</c>.</value>
        public bool EmailConfirmed { get; set; }
    }

    public enum Roles
    {
        Administrator,

        Teacher,

        Father,

        Children
    }
}
