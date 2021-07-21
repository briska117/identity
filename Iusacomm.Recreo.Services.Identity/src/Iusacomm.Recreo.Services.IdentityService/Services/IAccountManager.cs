using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iusacomm.Recreo.Services.IdentityService.Services
{
    public interface IAccountManager
    {
        Task<IdentityUser> CreateUser(string email, string requestScheme, string requestHost, string rol = IdentityConstants.Father);

        Task<IdentityUser> ResetPassword(string email, string requestScheme, string requestHost);
    }
}
