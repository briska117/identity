using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Iusacom.Tank.Services;
using Iusacom.Tank.Services.Customers;
using Iusacomm.Recreo.Services.IdentityService.Configuration;
using Iusacomm.Recreo.Services.IdentityService.Data;
using Iusacomm.Recreo.Services.IdentityService.Models;
using Iusacomm.Recreo.Services.IdentityService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Iusacomm.Recreo.Services.IdentityService.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICustomerService _customerService;
        private readonly IAccountManager _accountManager;
        private LoginSettings _loginSettings;

        public AccountController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ICustomerService customerService,
            IAccountManager accountManager,
             IOptions<LoginSettings> loginSettings)
        {
            _context = context;
            _userManager = userManager;
            _customerService = customerService;
            _accountManager = accountManager;
            _loginSettings = loginSettings.Value;
        }

        [HttpGet]
        [Authorize(
            Policy = IdentityConstants.ElevatedRightsPolicyName,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(BadRequestResult), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(NotFoundResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(MicroserviceError), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<List<IdentityUser>>> GetAccounts()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }

        /// <summary>
        /// Creates the customer acount.
        /// </summary>
        /// <param name="customerData">The customer data.</param>
        /// <returns></returns>
        [HttpPost("Customer")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BadRequestResult), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(NotFoundResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(MicroserviceError), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<string>> CreateCustomerAccount([FromBody]CustomerAcount customerData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var customer = await _customerService.GetCustomerByKit(customerData.Kit);

            if (!string.IsNullOrEmpty(customer.Email))
            {
                var userExist = await _userManager.FindByEmailAsync(customer.Email);

                if (userExist != null)
                {
                    return Conflict("kit already registered");
                }
            }

            var user = await _accountManager.CreateUser(customerData.Email, HttpContext.Request.Scheme, HttpContext.Request.Host.ToString());

            if (user == null)
            {
                return Conflict();
            }

            var custId = customer.Id;
            customer.Id = null;
            customer.Email = customerData.Email;

            await _customerService.UpdateCustomer(custId, customer);

            return Ok(customerData.Email);
        }
        /// <summary>
        /// Creates the account.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(
            Policy = IdentityConstants.AdminRightsPolicyName,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(BadRequestResult), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(NotFoundResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(MicroserviceError), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<string>> CreateAccount([FromBody]UserAccount account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var claimRole = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;

            if (claimRole == account.Role.ToString("G"))
            {
                return Unauthorized();
            }

            var user = await _accountManager.CreateUser(account.Email, HttpContext.Request.Scheme, HttpContext.Request.Host.ToString(), account.Role.ToString("G"));

            if (user == null)
            {
                return Conflict();
            }

            return Ok($"{account.Email} Confirmation Required");
        }

        /// <summary>
        /// Confirms the email.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        [HttpGet("ConfirmEmail/{userId}/{token}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BadRequestResult), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(NotFoundResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(MicroserviceError), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<string>> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (null == user)
            {
                return NotFound($"User Id {userId} not Found");
            }

            var decodeToken = WebUtility.UrlDecode(token);

            var result = await _userManager.ConfirmEmailAsync(user, decodeToken);

            if (!result.Succeeded)
            {
                return Conflict("Confirmation fail");
            }

            var url = $"{_loginSettings.Uri}{_loginSettings.Path}";

            return Redirect(url);
        }

        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        [HttpGet("ResetPassword")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BadRequestResult), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(NotFoundResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(MicroserviceError), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult<string>> ResetPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (null == user)
            {
                return NotFound($" {email} not Found");
            }

            user = await _accountManager.ResetPassword(email, HttpContext.Request.Scheme, HttpContext.Request.Host.ToString());

            if (null == user)
            {
                return Conflict("Reset fail");
            }

            return Ok(email);
        }

        /// <summary>
        /// Gets the account information.
        /// </summary>
        /// <returns></returns>
        [HttpGet("AccountInfo")]
        [Authorize(
            Policy = IdentityConstants.ElevatedRightsPolicyName,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(BadRequestResult), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(NotFoundResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(MicroserviceError), (int)HttpStatusCode.InternalServerError)]
        public ActionResult<dynamic> GetAccountInfo()
        {
            var userclaims = User.Claims;
            var email = userclaims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var role = userclaims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;

            if (null == email)
            {
                return NotFound($" User not Found");
            }

            var response = new
            {
                email,
                role
            };

            return Ok(response);
        }

      
    }
}
