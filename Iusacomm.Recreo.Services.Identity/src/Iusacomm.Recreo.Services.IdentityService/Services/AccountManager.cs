using Iusacomm.Recreo.Services.IdentityService.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Iusacomm.Recreo.Services.IdentityService.Services
{
    public class AccountManager:IAccountManager
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IEmailSender _emailSender;

        public AccountManager(
            UserManager<IdentityUser> userManager,
            IHostingEnvironment hostingEnvironment,
            ApplicationDbContext applicationDbContext,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
            _applicationDbContext = applicationDbContext;
            _emailSender = emailSender;
        }

        public async Task<IdentityUser> CreateUser(string email, string requestScheme, string requestHost, string role = IdentityConstants.Father)
        {
            var normalizedEmail = email.ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user != null)
            {
                throw new Exception("User already exists");
            }

            var randomPassword = Guid.NewGuid()
                   .ToString()
                   .Replace("-", string.Empty)
                   .Substring(0, 4)
                   .ToLowerInvariant();

            var generatedPassword = $"E8#j{randomPassword}";

            user = new IdentityUser { Email = email, UserName = email };

            var identityResult = await _userManager.CreateAsync(user, generatedPassword);
            ThrowIfFailedIdentityResult(identityResult);

            identityResult = await _userManager.AddToRoleAsync(user, role);
            ThrowIfFailedIdentityResult(identityResult);

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = $"{requestScheme}://{requestHost}/api/Account/ConfirmEmail/{user.Id}/{WebUtility.UrlEncode(confirmationToken)}";

            var htmlFileInfo = _hostingEnvironment.ContentRootFileProvider.GetFileInfo("Email/NewUser.html");
            var html = System.IO.File.ReadAllText(htmlFileInfo.PhysicalPath)
                    .Replace("{Email}", normalizedEmail)
                    .Replace("{Password}", generatedPassword)
                    .Replace("{CallbackUrl}", callbackUrl)
                    .Replace("{LogoUrl}", $"{requestScheme}://{requestHost}/images/email-logo.png")
                    .Replace("{SecondLogoUrl}", $"{requestScheme}://{requestHost}/images/email_logo_2.jpg");

            var textFileInfo = _hostingEnvironment.ContentRootFileProvider.GetFileInfo("Email/NewUser.txt");
            var text = System.IO.File.ReadAllText(textFileInfo.PhysicalPath)
                .Replace("{Email}", normalizedEmail)
                .Replace("{Password}", generatedPassword)
                .Replace("{CallbackUrl}", callbackUrl);

            await _emailSender.SendEmailAsync(
                    email: normalizedEmail,
                    subject: "Bienvenido a RECREO",
                    htmlMessage: html,
                    textMessage: text);

            await _applicationDbContext.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Throws if failed identity result.
        /// </summary>
        /// <param name="identityResult">The identity result.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void ThrowIfFailedIdentityResult(IdentityResult identityResult)
        {
            if (!identityResult.Succeeded)
            {
                var sb = new StringBuilder();
                foreach (var error in identityResult.Errors)
                {
                    sb.AppendLine($"({error.Code}) {error.Description}");
                }

                throw new InvalidOperationException(sb.ToString());
            }
        }

        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="requestScheme">The request scheme.</param>
        /// <param name="requestHost">The request host.</param>
        /// <returns></returns>
        public async Task<IdentityUser> ResetPassword(string email, string requestScheme, string requestHost)
        {
            var normalizedEmail = email.ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user == null)
            {
                return user;
            }

            var randomPassword = Guid.NewGuid()
                   .ToString()
                   .Replace("-", string.Empty)
                   .Substring(0, 4)
                   .ToLowerInvariant();

            var generatedPassword = $"E8#j{randomPassword}";

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var identityResult = await _userManager.ResetPasswordAsync(user, resetToken, generatedPassword);
            ThrowIfFailedIdentityResult(identityResult);

            user.EmailConfirmed = false;

            identityResult = await _userManager.UpdateAsync(user);
            ThrowIfFailedIdentityResult(identityResult);

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = $"{requestScheme}://{requestHost}/api/Account/ConfirmEmail/{user.Id}/{WebUtility.UrlEncode(confirmationToken)}";

            var htmlFileInfo = _hostingEnvironment.ContentRootFileProvider.GetFileInfo("Email/NewUser.html");
            var html = System.IO.File.ReadAllText(htmlFileInfo.PhysicalPath)
                    .Replace("{Email}", normalizedEmail)
                    .Replace("{Password}", generatedPassword)
                    .Replace("{CallbackUrl}", callbackUrl)
                    .Replace("{LogoUrl}", $"{requestScheme}://{requestHost}/images/email-logo.png");

            var textFileInfo = _hostingEnvironment.ContentRootFileProvider.GetFileInfo("Email/NewUser.txt");
            var text = System.IO.File.ReadAllText(textFileInfo.PhysicalPath)
                .Replace("{Email}", normalizedEmail)
                .Replace("{Password}", generatedPassword)
                .Replace("{CallbackUrl}", callbackUrl);

            await _emailSender.SendEmailAsync(
                    email: normalizedEmail,
                    subject: "Welcome to RECREO",
                    htmlMessage: html,
                    textMessage: text);

            await _applicationDbContext.SaveChangesAsync();

            return user;
        }
    }
}
