using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Iusacomm.Recreo.Services.IdentityService.Configuration;
using Iusacomm.Recreo.Services.IdentityService.Data;
using Iusacomm.Recreo.Services.IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Iusacomm.Recreo.Services.IdentityService.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private UserManager<IdentityUser> _userManager;
        private JwtSettings _jwtSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="jwtSettingsOptions">The JWT settings options.</param>
        public AuthenticationController(
            UserManager<IdentityUser> userManager,
            IOptions<JwtSettings> jwtSettingsOptions)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettingsOptions.Value;
        }

        /// <summary>
        /// Creates the token.
        /// </summary>
        /// <param name="tokenInfo">The token information.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns></returns>
        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<ActionResult> CreateToken(
            [FromBody] CreateToken tokenInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var applicationUser = await Authenticate(tokenInfo.Email, tokenInfo.Password);
            if (null == applicationUser || !applicationUser.EmailConfirmed)
            {
                return Unauthorized("Verifica tus datos de usuario");
            }

            var jwt = await GenerateToken(applicationUser);
            return Ok(new { token = jwt });
        }

        /// <summary>
        /// Authenticates the specified email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private async Task<IdentityUser> Authenticate(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (null == user)
            {
                return null;
            }

            if (await _userManager.CheckPasswordAsync(user, password))
            {
                return user;
            }

            return null;
        }

        /// <summary>
        /// Generates the token.
        /// </summary>
        /// <param name="applicationUser">The application user.</param>
        /// <returns></returns>
        private async Task<string> GenerateToken(IdentityUser applicationUser)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var userRoles = await _userManager.GetRolesAsync(applicationUser);

            var role = userRoles.FirstOrDefault();

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer),
                new Claim(JwtRegisteredClaimNames.Sub, applicationUser.Id),
                new Claim(ClaimTypes.Email, applicationUser.Email),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Issuer,
                claims: claims,
                expires: DateTime.Now.AddHours(_jwtSettings.AccessExpiration),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
