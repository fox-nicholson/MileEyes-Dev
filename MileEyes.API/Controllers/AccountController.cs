using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using MileEyes.API.Models;
using MileEyes.API.Models.DatabaseModels;
using MileEyes.API.Providers;
using MileEyes.API.Results;
using MileEyes.API.Services;

namespace MileEyes.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; }

        // GET api/Account/UserInfo
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            var externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            var userId = User.Identity.GetUserId();

            var user = db.Users.FirstOrDefault(u => u.Id == userId);

            try
            {
                return new UserInfoViewModel
                {
                    Email = User.Identity.GetUserName(),
                    HasRegistered = externalLogin == null,
                    LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null,
                    EmailConfirmed = user.EmailConfirmed,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PlaceId = user.Address.PlaceId
                };
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        [Route("UserInfo")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateUserInfo(UserInfoBindingModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.Identity.GetUserId();

            var user = db.Users.FirstOrDefault(u => u.Id == userId);

            try
            {
                user.Email = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                var existingAddresses = db.Addresses.Where(a => a.PlaceId == model.PlaceId);

                if (existingAddresses.Any())
                    user.Address = existingAddresses.FirstOrDefault();
                else
                    user.Address = new Address
                    {
                        PlaceId = model.PlaceId
                    };
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
            await db.SaveChangesAsync();

            return Ok();
        }

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
                return null;

            var logins = new List<UserLoginInfoViewModel>();

            foreach (var linkedAccount in user.Logins)
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });

            if (user.PasswordHash != null)
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName
                });

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
                return GetErrorResult(result);

            return Ok();
        }

        // POST api/Account/AddExternalLogin
        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            var ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow)
                return BadRequest("External login failure.");

            var externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
                return BadRequest("The external login is already associated with an account.");

            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
                return GetErrorResult(result);

            return Ok();
        }

        // POST api/Account/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            else
                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));

            if (!result.Succeeded)
                return GetErrorResult(result);

            return Ok();
        }

        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));

            if (!User.Identity.IsAuthenticated)
                return new ChallengeResult(provider, this);

            var externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
                return InternalServerError();

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            var user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            var hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                var oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    OAuthDefaults.AuthenticationType);
                var cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                var properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                var identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            var descriptions = Authentication.GetExternalAuthenticationTypes();
            var logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (var description in descriptions)
            {
                var login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingAddress = db.Addresses.FirstOrDefault(a => a.PlaceId == model.PlaceId);

            Address address;

            if (existingAddress == null)
            {
                var addressResult = await GeocodingService.GetAddress(model.PlaceId);

                address = new Address
                {
                    Id = Guid.NewGuid(),
                    PlaceId = addressResult.PlaceId,
                    Coordinates = new Coordinates
                    {
                        Id = Guid.NewGuid(),
                        Latitude = addressResult.Latitude,
                        Longitude = addressResult.Longitude
                    }
                };
            }
            else
            {
                address = existingAddress;
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return GetErrorResult(result);

            var dbuser = db.Users.Find(user.Id);

            dbuser.Address = address;

            var owner = new Owner
            {
                Id = Guid.NewGuid()
            };
            var accountant = new Accountant
            {
                Id = Guid.NewGuid()
            };
            var manager = new Manager
            {
                Id = Guid.NewGuid()
            };
            var driver = new Driver
            {
                Id = Guid.NewGuid()
            };

            var personalCompany = new Company
            {
                Id = Guid.NewGuid(),
                Name = "Personal",
                AutoAccept = true,
                HighRate = 0.45M,
                LowRate = 0.25M,
                Address = address,
                AutoAcceptDistance = 160934,
                Personal = true,
                Vat = false
            };

            personalCompany.Profiles.Add(driver);

            owner.Companies.Add(personalCompany);

            dbuser.Profiles.Add(owner);
            dbuser.Profiles.Add(accountant);
            dbuser.Profiles.Add(manager);
            dbuser.Profiles.Add(driver);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }
            var hostname = Request.RequestUri.Host;

            //var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            //var callbackUrl = "http://" + hostname + "/Account/ConfirmEmail?userId=" + user.Id +
            //                  "&code=" + code;

            //await UserManager.SendEmailAsync(user.Id, "Welcome to MileEyes", callbackUrl);

            return Ok();
        }

        // GET api/Account/CheckEmail
        [AllowAnonymous]
        [Route("CheckEmail")]
        [HttpGet]
		public async Task<IHttpActionResult> CheckEmail(String email)
		{
			var result = UserManager.FindByEmail(email);
			if (result != null)
			{
				return BadRequest("The email address has been taken!");
			}
			return Ok();
		}

		// GET api/Account/CheckInvite
		[OverrideAuthentication]
		[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
		[Route("CheckDriverInvite")]
		[HttpGet]
		public async Task<IHttpActionResult> CheckDriverInvite(String companyId, String email)
		{
			var company = db.Companies.FirstOrDefault(c => c.Id.ToString() == companyId);
			if (company == null)
			{
				return BadRequest();
			}
			var user = db.Users.Find(User.Identity.GetUserId());
			var owner = user.Profiles.OfType<Owner>().FirstOrDefault();
			var manager = user.Profiles.OfType<Manager>().FirstOrDefault();

			var companyOwners = company.Profiles.OfType<Owner>();
			var companyManagers = company.Profiles.OfType<Manager>();

			if (!companyOwners.Contains(owner) && !companyManagers.Contains(manager))
			{
				return BadRequest();
			}
			var driverInvite = db.DriverInvites.FirstOrDefault(i => i.Company.Id.ToString() == companyId && i.Email.ToLower() == email.ToLower());
			if (driverInvite != null)
			{
				return BadRequest();
			}
			return Ok();
		}

		[OverrideAuthentication]
		[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
		[Route("AcceptDriverInviteEmail")]
		[HttpPost]
		public async Task<IHttpActionResult> AcceptDriverInviteEmail(String invite, String email)
		{
			var user = db.Users.Find(User.Identity.GetUserId());
			if (user.Email.ToLower() != email.ToLower())
			{
				return BadRequest();
			}
			var driverInvite = db.DriverInvites.FirstOrDefault(i => i.Id.ToString() == invite && i.Email.ToLower() == email.ToLower());
			if (driverInvite == null)
			{
				return BadRequest("Invite isn't valid anymore!");
			}

			var company = db.Companies.FirstOrDefault(c => c.Id == driverInvite.Company.Id);
			if (company == null)
			{
				return BadRequest("Unable to find company!");
			}

			var driver = new Driver
			{
				Id = Guid.NewGuid()
			};

			driverInvite.Company.Profiles.Add(driver);

			db.DriverInvites.Remove(driverInvite);
			db.SaveChanges();
			return Ok();
		}

		[OverrideAuthentication]
		[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
		[Route("SendDriverInviteEmail")]
		[HttpPost]
		public async Task<IHttpActionResult> SendDriverInviteEmail(String companyId, String email)
		{
			var company = db.Companies.FirstOrDefault(c => c.Id.ToString() == companyId);
			if (company == null)
			{
				return BadRequest();
			}
			var user = db.Users.Find(User.Identity.GetUserId());
			var owner = user.Profiles.OfType<Owner>().FirstOrDefault();
			var manager = user.Profiles.OfType<Manager>().FirstOrDefault();

			var companyOwners = company.Profiles.OfType<Owner>();
			var companyManagers = company.Profiles.OfType<Manager>();

			if (!companyOwners.Contains(owner) && !companyManagers.Contains(manager))
			{
				return BadRequest();
			}
			var driverInvite = db.DriverInvites.FirstOrDefault(i => i.Company.Id.ToString() == companyId && i.Email.ToLower() == email.ToLower());
			if (driverInvite != null)
			{
				return BadRequest();
			}

			UserManager.SendEmail(user.Id, "You have been invited to be driver for " + company.Name, "Hello World");

			return Ok("Invite has been sent!");
		}

		//ResendDriverInviteEmai;

        // POST api/Account/RegisterExternal
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("RegisterExternal")]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
                return InternalServerError();

            var user = new ApplicationUser {UserName = model.Email, Email = model.Email};

            var result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
                return GetErrorResult(result);

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
                return GetErrorResult(result);
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
                return InternalServerError();

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error);

                if (ModelState.IsValid)
                    return BadRequest();

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                    return null;

                var providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || string.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || string.IsNullOrEmpty(providerKeyClaim.Value))
                    return null;

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                    return null;

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static readonly RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");

                var strengthInBytes = strengthInBits / bitsPerByte;

                var data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
}