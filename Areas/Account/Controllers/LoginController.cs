using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using OpeniT.PowerbiDashboardApp.Data;
using OpeniT.PowerbiDashboardApp.Helpers;
using OpeniT.PowerbiDashboardApp.Models.Accounts;
using OpeniT.PowerbiDashboardApp.Models.Application;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Areas.Account.Controllers
{
	[Area("Account")]
	[AllowAnonymous]
	public class LoginController : Controller
	{
		private string ControllerName = nameof(LoginController);

		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly UserManager<ApplicationUser> userManager;
		private readonly ApplicationLogger logger;
		private readonly DataRepository dataRepository;

		public LoginController(SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			ApplicationLogger logger,
			DataRepository dataRepository)
		{
			this.signInManager = signInManager;
			this.userManager = userManager;
			this.logger = logger;
			this.dataRepository = dataRepository;
		}

		[Route("[controller]")]
		[Route("[area]/[controller]")]
		public async Task<IActionResult> Index(string returnUrl)
		{
            var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"{ControllerName}.Index/{returnUrl}" };

            var owner = this.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
			if (owner != null)
			{
				return LocalRedirect("/");
			}

            var ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
			var redirectUrl = Url.Action("ExternalLoginCallback", "Login", new { area = "Account", ReturnUrl = returnUrl });
			var properties = signInManager.ConfigureExternalAuthenticationProperties(ExternalLogins[0].Name, redirectUrl);

            await logger.LogNavigation(activity: activity, log: $"Authentication challenged from Url [{returnUrl}]");
			return new ChallengeResult(ExternalLogins[0].Name, properties);
        }

		public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
		{
			var activity = new ApplicationActivity() { Controller = ControllerName, Action = $"{ControllerName}.ExternalLoginCallback/{returnUrl}/{remoteError}" };
			activity.LogType = Site.ConstantValues.LogLoginActivity;

			returnUrl = returnUrl ?? Url.Content("~/");

			if (remoteError != null)
			{
				activity.Log = $"External Provider Login Error [{remoteError}] from URL [{returnUrl}]";
				await logger.Log(activity);

				ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
				return LocalRedirect(returnUrl);
			}

			// Get the login information about the user from the external login provider
			var info = await signInManager.GetExternalLoginInfoAsync();

			if (info == null)
			{
				activity.Log = $"Error loading external login information from URL [{returnUrl}]";
				await logger.Log(activity);

				ModelState.AddModelError(string.Empty, "Error loading external login information.");
				return LocalRedirect(returnUrl);
			}

			// Get the claim values
			var referenceId = info.Principal.FindFirstValue(ClaimConstants.ObjectId);
			var email = info.Principal.FindFirstValue(ClaimTypes.Name);
			var displayName = info.Principal.FindFirstValue(ClaimConstants.Name);
			var roles = info.Principal.FindAll(ClaimTypes.Role).Select(x => x.Value);
			var roleString = string.Join(',', roles);

			// If the user already has a login (i.e if there is a record in AspNetUserLogins table) then sign-in the user with this external login provider
			var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

			if (signInResult.Succeeded)
			{
				var identityName = signInManager.Context?.User?.Identity?.Name;
				activity.Log = $"Remote Provider [{info.LoginProvider}] Key [{info.ProviderKey}]: Login Success by identity [{identityName}] with email [{email}] of role [{roleString}]";
				await logger.Log(activity);

				var applicationUser = await TryGetUser(info, activity);
				if (applicationUser == null)
				{
					await signInManager.SignOutAsync();
					return LocalRedirect("/");
				}

				if (applicationUser.InternalAccount == null)
				{
					applicationUser.InternalAccount = new InternalAccount() { ReferenceId = referenceId, DisplayName = displayName, Email = email };
					await this.dataRepository.UpdateApplicationUser(applicationUser);
				}

				var updated = UpdateInfoIfMismatch(applicationUser, info);
				if (updated)
				{
					await this.dataRepository.UpdateApplicationUser(applicationUser);
				}

				if (roles.Any())
				{
					await this.dataRepository.SetToRoles(email, roles);
				}
				else
				{
					await this.dataRepository.SetToRole(email, Site.ConstantValues.InternalUser);
				}

				return LocalRedirect(returnUrl);
			}
			// If there is no record in AspNetUserLogins table, the user may not have a local account
			else
			{
				if (email != null)
				{
					// Create a new user without password if we do not have a user already
					var user = await this.dataRepository.GetApplicationUserByEmail(email);
					if (user == null)
					{
						var internalAccount = new InternalAccount() { ReferenceId = referenceId, DisplayName = displayName, Email = email };
						user = await this.dataRepository.AddApplicationUserInternalAccountAsync(internalAccount);

						activity.Log = $"Remote Provider Login: Created email [{email}] of role [{roleString}]";
						await logger.Log(activity);
					}

					if (roles.Any())
					{
						await this.dataRepository.AddToRoles(user, roles);
					}
					else
					{
						await this.dataRepository.AddToRole(email, Site.ConstantValues.InternalUser);
					}

					// Add a login (i.e insert a row for the user in AspNetUserLogins table)
					await userManager.AddLoginAsync(user, info);
					await signInManager.SignInAsync(user, isPersistent: false);

					activity.Log = $"Remote Provider Login Success: First Login by email [{email}] of role [{roleString}]";
					await logger.Log(activity);
				}
			}
			return LocalRedirect(returnUrl);
		}

		private async Task<ApplicationUser> TryGetUser(ExternalLoginInfo info, ApplicationActivity activity)
		{
			var referenceId = info.Principal.FindFirstValue(ClaimConstants.ObjectId);
			var email = info.Principal.FindFirstValue(ClaimTypes.Name);
			var identityName = signInManager.Context?.User?.Identity?.Name;

			var applicationUser = await this.dataRepository.GetApplicationUserByProviderKey(info.ProviderKey);
			if (applicationUser != null)
			{
				return applicationUser;
			}
			else
			{
				activity.Log = $"Failed to fetch application user identity [{identityName}] with email [{email}] with id [{referenceId}] by Key [{info.ProviderKey}]";
				await logger.Log(activity);
			}

			applicationUser = await this.dataRepository.GetApplicationUserWithAccountByEmail(email);
			if (applicationUser != null)
			{
				return applicationUser;
			}
			else
			{
				activity.Log = $"Failed to fetch application user identity [{identityName}] with email [{email}] with id [{referenceId}] by Email";
				await logger.Log(activity);
			}

			applicationUser = await this.dataRepository.GetInternalUserByReferenceId(email);
			if (applicationUser != null)
			{
				return applicationUser;
			}
			else
			{
				activity.Log = $"Failed to fetch application user identity [{identityName}] with email [{email}] with id [{referenceId}] by ReferenceId";
				await logger.Log(activity);
			}

			return null;
		}

		private bool UpdateInfoIfMismatch(ApplicationUser applicationUser, ExternalLoginInfo info)
		{
			bool mismatch = false;

			var email = info.Principal.FindFirstValue(ClaimTypes.Name);
			if (applicationUser.Email != email)
			{
				mismatch = true;
				applicationUser.Email = email;
			}

			if (applicationUser.InternalAccount.Email != email)
			{
				mismatch = true;
				applicationUser.InternalAccount.Email = email;
			}

			var displayName = info.Principal.FindFirstValue(ClaimConstants.Name);
			if (applicationUser.InternalAccount.DisplayName != displayName)
			{
				mismatch = true;
				applicationUser.InternalAccount.DisplayName = displayName;
			}

			var referenceId = info.Principal.FindFirstValue(ClaimConstants.ObjectId);
			if (applicationUser.InternalAccount.ReferenceId != referenceId)
			{
				mismatch = true;
				applicationUser.InternalAccount.ReferenceId = referenceId;
			}


			return mismatch;
		}
	}
}
