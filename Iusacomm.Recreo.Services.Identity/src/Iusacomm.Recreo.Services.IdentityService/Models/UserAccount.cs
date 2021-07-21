using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Iusacomm.Recreo.Services.IdentityService.Models
{
    public class UserAccount
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public Roles Role { get; set; }
    }
}
