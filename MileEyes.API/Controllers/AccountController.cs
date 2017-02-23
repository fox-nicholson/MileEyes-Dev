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
using MileEyes.API.App_Start;
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
            // "&code=" + code;

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

        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("CheckUserRight")]
        [HttpGet]
        public async Task<IHttpActionResult> CheckUserRight(String companyId)
        {
            var company = db.Companies.FirstOrDefault(c => c.Id.ToString() == companyId);
            if (company == null)
            {
                return BadRequest("Unable to find company!");
            }
            var user = db.Users.Find(User.Identity.GetUserId());

            var owner = user.Profiles.OfType<Owner>();
            var companyOwners = company.Profiles.OfType<Owner>();
            foreach (var o in owner)
            {
                if (companyOwners.Contains(o))
                {
                    return Ok("4");
                }
            }
            var accountant = user.Profiles.OfType<Accountant>();
            var companyAccountants = company.Profiles.OfType<Accountant>();
            foreach (var a in accountant)
            {
                if (companyAccountants.Contains(a))
                {
                    return Ok("3");
                }
            }
            var manager = user.Profiles.OfType<Manager>();
            var companyManagers = company.Profiles.OfType<Manager>();
            foreach (var m in manager)
            {
                if (companyManagers.Contains(m))
                {
                    return Ok("2");
                }
            }
            var driver = user.Profiles.OfType<Driver>();
            var companyDrivers = company.Profiles.OfType<Driver>();
            foreach (var d in driver)
            {
                if (companyDrivers.Contains(d))
                {
                    return Ok("1");
                }
            }
            return BadRequest("You don't have the rights to use this.");
        }

        // GET api/Account/CheckInvite
        [AllowAnonymous]
        [Route("CheckDriverInvite")]
        [HttpGet]
        public async Task<IHttpActionResult> CheckDriverInvite(String companyId, String email)
        {
            var company = db.Companies.FirstOrDefault(c => c.Id.ToString() == companyId);
            if (company == null)
            {
                return BadRequest("Unable to find company!");
            }
            var driverInvite =
                db.DriverInvites.FirstOrDefault(
                    i => i.Company.Id.ToString() == companyId && i.Email.ToLower() == email.ToLower());
            if (driverInvite == null)
            {
                return BadRequest("Unable to find driver invite!");
            }
            return Ok();
        }

        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("AcceptDriverInviteEmail")]
        [HttpPost]
        public async Task<IHttpActionResult> AcceptDriverInviteEmail(String invite, String email)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            if (user.Email.ToLower() != email.ToLower())
            {
                return BadRequest("Invite isn't for you!");
            }
            var driverInvite =
                db.DriverInvites.FirstOrDefault(i => i.Id.ToString() == invite && i.Email.ToLower() == email.ToLower());
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
                Id = Guid.NewGuid(),
                User = user
            };

            company.Profiles.Add(driver);

            db.DriverInvites.Remove(driverInvite);
            db.SaveChanges();
            return Ok();
        }

        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("SendDriverInviteEmail")]
        [HttpPost]
        public async Task<IHttpActionResult> SendDriverInviteEmail(String companyId, String email)
        {
            var company = db.Companies.FirstOrDefault(c => c.Id.ToString() == companyId);
            if (company == null)
            {
                return BadRequest("Unable to find company!");
            }
            var user = db.Users.Find(User.Identity.GetUserId());
            var owner = user.Profiles.OfType<Owner>().FirstOrDefault();
            var manager = user.Profiles.OfType<Manager>().FirstOrDefault();

            var companyOwners = company.Profiles.OfType<Owner>();
            var companyManagers = company.Profiles.OfType<Manager>();

            if (!companyOwners.Contains(owner) && !companyManagers.Contains(manager))
            {
                return BadRequest("You don't have the rights to do this.");
            }
            var driverInvite = db.DriverInvites.FirstOrDefault(i => i.Company.Id.ToString() == companyId && i.Email.ToLower() == email.ToLower());
            if (driverInvite != null)
            {
                db.DriverInvites.Remove(driverInvite);
            }
            var otherUser = UserManager.FindByEmail(email);
            if (otherUser != null)
            {
                var drivers = company.Profiles.OfType<Driver>();
                foreach (var d in drivers)
                {
                    if (d.User.Id == otherUser.Id)
                    {
                        return BadRequest("The driver is already in the company.");
                    }
                }

            }
            var invite = new DriverInvite
            {
                Company = company,
                Id = Guid.NewGuid(),
                Email = email
            };
            db.DriverInvites.Add(invite);
            db.SaveChanges();
            EmailService.SendEmail(email, company.Name + " - Driver Invite!",
                "<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Strict//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd'><html xmlns='http://www.w3.org/1999/xhtml'> <head> <meta http-equiv='Content-Type' content='text/html; charset=utf-8'> <meta name='viewport' content='width=device-width'> <style> #outlook a { padding: 0; } body { width: 100% !important; min-width: 100%; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; margin: 0; Margin: 0; padding: 0; -moz-box-sizing: border-box; -webkit-box-sizing: border-box; box-sizing: border-box; } .ExternalClass { width: 100%; }" +
                " .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div { line-height: 100%; } #backgroundTable { margin: 0; Margin: 0; padding: 0; width: 100% !important; line-height: 100% !important; } img { outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; width: auto; max-width: 100%; clear: both; display: block; } center { width: 100%; min-width: 580px; } a img { border: none; } a.button {	 background-color: #47a800;	 color: white;	 padding: 1em;	 border-radius: 0.25em;	 display: inline-block;	 margin: 2em 0; }" +
                " a.button:hover {	 background-color: #72dd25;	 color: white; } a.button:visited {	 background-color: #72dd25;	 color: white; } .button-row {	 text-align: center; } p { margin: 0 0 0 10px; Margin: 0 0 0 10px; } table { border-spacing: 0; border-collapse: collapse; } td { border-collapse: collapse !important; } table, tr, td { padding: 0; vertical-align: top; text-align: left; } html { min-height: 100%; background: #14b9ee; } table.body { background: #14b9ee; height: 100%; width: 100%; } table.container { background: white; width: 580px; margin: 0 auto; Margin: 0 auto; text-align: inherit; }" +
                " table.row { padding: 0; width: 100%; position: relative; } table.container table.row { display: table; } td.columns, td.column, th.columns, th.column { margin: 0 auto; Margin: 0 auto; padding-left: 16px; padding-bottom: 16px; } td.columns.last, td.column.last, th.columns.last, th.column.last { padding-right: 16px; } td.columns table, td.column table, th.columns table, th.column table { width: 100%; } td.large-1, th.large-1 { width: 32.33333px; padding-left: 8px; padding-right: 8px; } td.large-1.first, th.large-1.first { padding-left: 16px; } td.large-1.last, th.large-1.last { padding-right: 16px; }" +
                " .collapse > tbody > tr > td.large-1, .collapse > tbody > tr > th.large-1 { padding-right: 0; padding-left: 0; width: 48.33333px; } .collapse td.large-1.first, .collapse th.large-1.first, .collapse td.large-1.last, .collapse th.large-1.last { width: 56.33333px; } td.large-2, th.large-2 { width: 80.66667px; padding-left: 8px; padding-right: 8px; } td.large-2.first, th.large-2.first { padding-left: 16px; } td.large-2.last, th.large-2.last { padding-right: 16px; } .collapse > tbody > tr > td.large-2, .collapse > tbody > tr > th.large-2 { padding-right: 0; padding-left: 0; width: 96.66667px; }" +
                " .collapse td.large-2.first, .collapse th.large-2.first, .collapse td.large-2.last, .collapse th.large-2.last { width: 104.66667px; } td.large-3, th.large-3 { width: 129px; padding-left: 8px; padding-right: 8px; } td.large-3.first, th.large-3.first { padding-left: 16px; } td.large-3.last, th.large-3.last { padding-right: 16px; } .collapse > tbody > tr > td.large-3, .collapse > tbody > tr > th.large-3 { padding-right: 0; padding-left: 0; width: 145px; } .collapse td.large-3.first, .collapse th.large-3.first, .collapse td.large-3.last, .collapse th.large-3.last { width: 153px; } td.large-4, th.large-4 " +
                "{ width: 177.33333px; padding-left: 8px; padding-right: 8px; } td.large-4.first, th.large-4.first { padding-left: 16px; } td.large-4.last, th.large-4.last { padding-right: 16px; } .collapse > tbody > tr > td.large-4, .collapse > tbody > tr > th.large-4 { padding-right: 0; padding-left: 0; width: 193.33333px; } .collapse td.large-4.first, .collapse th.large-4.first, .collapse td.large-4.last, .collapse th.large-4.last { width: 201.33333px; } td.large-5, th.large-5 { width: 225.66667px; padding-left: 8px; padding-right: 8px; } td.large-5.first, th.large-5.first { padding-left: 16px; } td.large-5.last, th.large-5.last " +
                "{ padding-right: 16px; } .collapse > tbody > tr > td.large-5, .collapse > tbody > tr > th.large-5 { padding-right: 0; padding-left: 0; width: 241.66667px; } .collapse td.large-5.first, .collapse th.large-5.first, .collapse td.large-5.last, .collapse th.large-5.last { width: 249.66667px; } td.large-6, th.large-6 { width: 274px; padding-left: 8px; padding-right: 8px; } td.large-6.first, th.large-6.first { padding-left: 16px; } td.large-6.last, th.large-6.last { padding-right: 16px; } .collapse > tbody > tr > td.large-6, .collapse > tbody > tr > th.large-6 { padding-right: 0; padding-left: 0; width: 290px; } .collapse td.large-6.first," +
                " .collapse th.large-6.first, .collapse td.large-6.last, .collapse th.large-6.last { width: 298px; } td.large-7, th.large-7 { width: 322.33333px; padding-left: 8px; padding-right: 8px; } td.large-7.first, th.large-7.first { padding-left: 16px; } td.large-7.last, th.large-7.last { padding-right: 16px; } .collapse > tbody > tr > td.large-7, .collapse > tbody > tr > th.large-7 { padding-right: 0; padding-left: 0; width: 338.33333px; } .collapse td.large-7.first, .collapse th.large-7.first, .collapse td.large-7.last, .collapse th.large-7.last { width: 346.33333px; } td.large-8, th.large-8 { width: 370.66667px; padding-left: 8px; padding-right: 8px; }" +
                " td.large-8.first, th.large-8.first { padding-left: 16px; } td.large-8.last, th.large-8.last { padding-right: 16px; } .collapse > tbody > tr > td.large-8, .collapse > tbody > tr > th.large-8 { padding-right: 0; padding-left: 0; width: 386.66667px; } .collapse td.large-8.first, .collapse th.large-8.first, .collapse td.large-8.last, .collapse th.large-8.last { width: 394.66667px; } td.large-9, th.large-9 { width: 419px; padding-left: 8px; padding-right: 8px; } td.large-9.first, th.large-9.first { padding-left: 16px; } td.large-9.last, th.large-9.last { padding-right: 16px; } .collapse > tbody > tr > td.large-9, .collapse > tbody > tr > th.large-9 " +
                "{ padding-right: 0; padding-left: 0; width: 435px; } .collapse td.large-9.first, .collapse th.large-9.first, .collapse td.large-9.last, .collapse th.large-9.last { width: 443px; } td.large-10, th.large-10 { width: 467.33333px; padding-left: 8px; padding-right: 8px; } td.large-10.first, th.large-10.first { padding-left: 16px; } td.large-10.last, th.large-10.last { padding-right: 16px; } .collapse > tbody > tr > td.large-10, .collapse > tbody > tr > th.large-10 { padding-right: 0; padding-left: 0; width: 483.33333px; } .collapse td.large-10.first, .collapse th.large-10.first, .collapse td.large-10.last, .collapse th.large-10.last { width: 491.33333px; " +
                "} td.large-11, th.large-11 { width: 515.66667px; padding-left: 8px; padding-right: 8px; } td.large-11.first, th.large-11.first { padding-left: 16px; } td.large-11.last, th.large-11.last { padding-right: 16px; } .collapse > tbody > tr > td.large-11, .collapse > tbody > tr > th.large-11 { padding-right: 0; padding-left: 0; width: 531.66667px; } .collapse td.large-11.first, .collapse th.large-11.first, .collapse td.large-11.last, .collapse th.large-11.last { width: 539.66667px; } td.large-12, th.large-12 { width: 564px; padding-left: 8px; padding-right: 8px; } td.large-12.first, th.large-12.first { padding-left: 16px; } td.large-12.last, th.large-12.last { " +
                "padding-right: 16px; } .collapse > tbody > tr > td.large-12, .collapse > tbody > tr > th.large-12 { padding-right: 0; padding-left: 0; width: 580px; } .collapse td.large-12.first, .collapse th.large-12.first, .collapse td.large-12.last, .collapse th.large-12.last { width: 588px; } td.large-1 center, th.large-1 center { min-width: 0.33333px; } td.large-2 center, th.large-2 center { min-width: 48.66667px; } td.large-3 center, th.large-3 center { min-width: 97px; } td.large-4 center, th.large-4 center { min-width: 145.33333px; } td.large-5 center, th.large-5 center { min-width: 193.66667px; } td.large-6 center, th.large-6 center { min-width: 242px; } " +
                "td.large-7 center, th.large-7 center { min-width: 290.33333px; } td.large-8 center, th.large-8 center { min-width: 338.66667px; } td.large-9 center, th.large-9 center { min-width: 387px; } td.large-10 center, th.large-10 center { min-width: 435.33333px; } td.large-11 center, th.large-11 center { min-width: 483.66667px; } td.large-12 center, th.large-12 center { min-width: 532px; } .body .columns td.large-1, .body .column td.large-1, .body .columns th.large-1, .body .column th.large-1 { width: 8.33333%; } .body .columns td.large-2, .body .column td.large-2, .body .columns th.large-2, .body .column th.large-2 { width: 16.66667%; } .body .columns td.large-3," +
                " .body .column td.large-3, .body .columns th.large-3, .body .column th.large-3 { width: 25%; } .body .columns td.large-4, .body .column td.large-4, .body .columns th.large-4, .body .column th.large-4 { width: 33.33333%; } .body .columns td.large-5, .body .column td.large-5, .body .columns th.large-5, .body .column th.large-5 { width: 41.66667%; } .body .columns td.large-6, .body .column td.large-6, .body .columns th.large-6, .body .column th.large-6 { width: 50%; } .body .columns td.large-7, .body .column td.large-7, .body .columns th.large-7, .body .column th.large-7 { width: 58.33333%; } .body .columns td.large-8, .body .column td.large-8, .body .columns th.large-8," +
                " .body .column th.large-8 { width: 66.66667%; } .body .columns td.large-9, .body .column td.large-9, .body .columns th.large-9, .body .column th.large-9 { width: 75%; } .body .columns td.large-10, .body .column td.large-10, .body .columns th.large-10, .body .column th.large-10 { width: 83.33333%; } .body .columns td.large-11, .body .column td.large-11, .body .columns th.large-11, .body .column th.large-11 { width: 91.66667%; } .body .columns td.large-12, .body .column td.large-12, .body .columns th.large-12, .body .column th.large-12 { width: 100%; } td.large-offset-1, td.large-offset-1.first, td.large-offset-1.last, th.large-offset-1, th.large-offset-1.first, th.large-offset-1.last " +
                "{ padding-left: 64.33333px; } td.large-offset-2, td.large-offset-2.first, td.large-offset-2.last, th.large-offset-2, th.large-offset-2.first, th.large-offset-2.last { padding-left: 112.66667px; } td.large-offset-3, td.large-offset-3.first, td.large-offset-3.last, th.large-offset-3, th.large-offset-3.first, th.large-offset-3.last { padding-left: 161px; } td.large-offset-4, td.large-offset-4.first, td.large-offset-4.last, th.large-offset-4, th.large-offset-4.first, th.large-offset-4.last { padding-left: 209.33333px; } td.large-offset-5, td.large-offset-5.first, td.large-offset-5.last, th.large-offset-5, th.large-offset-5.first, th.large-offset-5.last { padding-left: 257.66667px; }" +
                " td.large-offset-6, td.large-offset-6.first, td.large-offset-6.last, th.large-offset-6, th.large-offset-6.first, th.large-offset-6.last { padding-left: 306px; } td.large-offset-7, td.large-offset-7.first, td.large-offset-7.last, th.large-offset-7, th.large-offset-7.first, th.large-offset-7.last { padding-left: 354.33333px; } td.large-offset-8, td.large-offset-8.first, td.large-offset-8.last, th.large-offset-8, th.large-offset-8.first, th.large-offset-8.last { padding-left: 402.66667px; } td.large-offset-9, td.large-offset-9.first, td.large-offset-9.last, th.large-offset-9, th.large-offset-9.first, th.large-offset-9.last { padding-left: 451px; } td.large-offset-10, td.large-offset-10.first," +
                " td.large-offset-10.last, th.large-offset-10, th.large-offset-10.first, th.large-offset-10.last { padding-left: 499.33333px; } td.large-offset-11, td.large-offset-11.first, td.large-offset-11.last, th.large-offset-11, th.large-offset-11.first, th.large-offset-11.last { padding-left: 547.66667px; } td.expander, th.expander { visibility: hidden; width: 0; padding: 0 !important; } .block-grid { width: 100%; max-width: 580px; } .block-grid td { display: inline-block; padding: 8px; } .up-2 td { width: 274px !important; } .up-3 td { width: 177px !important; } .up-4 td { width: 129px !important; } .up-5 td { width: 100px !important;" +
                " } .up-6 td { width: 80px !important; } .up-7 td { width: 66px !important; } .up-8 td { width: 56px !important; } table.text-center, td.text-center, h1.text-center, h2.text-center, h3.text-center, h4.text-center, h5.text-center, h6.text-center, p.text-center, span.text-center { text-align: center; } h1.text-left, h2.text-left, h3.text-left, h4.text-left, h5.text-left, h6.text-left, p.text-left, span.text-left { text-align: left; } h1.text-right, h2.text-right, h3.text-right, h4.text-right, h5.text-right, h6.text-right, p.text-right, span.text-right { text-align: right; } span.text-center { display: block; width: 100%; text-align: center;" +
                " } @media only screen and (max-width: 596px) { .small-float-center { margin: 0 auto !important; float: none !important; text-align: center !important; } .small-text-center { text-align: center !important; } .small-text-left { text-align: left !important; } .small-text-right { text-align: right !important; } } img.float-left { float: left; text-align: left; } img.float-right { float: right; text-align: right; } img.float-center, img.text-center { margin: 0 auto; Margin: 0 auto; float: none; text-align: center; } table.float-center, td.float-center, th.float-center { margin: 0 auto; Margin: 0 auto; float: none; text-align: center; }" +
                " table.body table.container .hide-for-large { display: none; width: 0; mso-hide: all; overflow: hidden; max-height: 0px; font-size: 0; width: 0px; line-height: 0; } @media only screen and (max-width: 596px) { table.body table.container .hide-for-large { display: block !important; width: auto !important; overflow: visible !important; } } table.body table.container .hide-for-large * { mso-hide: all; } @media only screen and (max-width: 596px) { table.body table.container .row.hide-for-large, table.body table.container .row.hide-for-large { display: table !important; width: 100% !important; } } @media only screen and (max-width: 596px)" +
                " { table.body table.container .show-for-large { display: none !important; width: 0; mso-hide: all; overflow: hidden; } } body, table.body, h1, h2, h3, h4, h5, h6, p, td, th, a { color: #0a0a0a; font-family: 'Avenir', Helvetica, Arial, sans-serif; font-weight: normal; padding: 0; margin: 0; Margin: 0; text-align: left; line-height: 1.3; } h1, h2, h3, h4, h5, h6 { color: inherit; word-wrap: normal; font-family: 'Avenir', Helvetica, Arial, sans-serif; font-weight: normal; margin-bottom: 10px; Margin-bottom: 10px; } h1 { font-size: 34px; } h2 { font-size: 30px; } h3 { font-size: 28px; } h4 { font-size: 24px; } h5 { font-size: 20px; } h6 " +
                "{ font-size: 18px; } body, table.body, p, td, th { font-size: 16px; line-height: 19px; } p { margin-bottom: 10px; Margin-bottom: 10px; } p.lead { font-size: 20px; line-height: 1.6; } p.subheader { margin-top: 4px; margin-bottom: 8px; Margin-top: 4px; Margin-bottom: 8px; font-weight: normal; line-height: 1.4; color: #8a8a8a; } small { font-size: 80%; color: #cacaca; } a { color: #2199e8; text-decoration: none; } a:hover { color: #147dc2; } a:active { color: #147dc2; } a:visited { color: #2199e8; } h1 a, h1 a:visited, h2 a, h2 a:visited, h3 a, h3 a:visited, h4 a, h4 a:visited, h5 a, h5 a:visited, h6 a, h6 a:visited { color: #2199e8; " +
                "} pre { background: #f3f3f3; margin: 30px 0; Margin: 30px 0; } pre code { color: #cacaca; } pre code span.callout { color: #8a8a8a; font-weight: bold; } pre code span.callout-strong { color: #ff6908; font-weight: bold; } hr { max-width: 580px; height: 0; border-right: 0; border-top: 0; border-bottom: 1px solid #cacaca; border-left: 0; margin: 20px auto; Margin: 20px auto; clear: both; } .stat { font-size: 40px; line-height: 1; } p + .stat { margin-top: -16px; Margin-top: -16px; } table.button { width: auto !important; margin: 0 0 16px 0; Margin: 0 0 16px 0; } table.button table td { width: auto !important; text-align: left; color: #fefefe;" +
                " background: #2199e8; border: 2px solid #2199e8; } table.button table td.radius { border-radius: 3px; } table.button table td.rounded { border-radius: 500px; } table.button table td a { font-family: 'Avenir', Helvetica, Arial, sans-serif; font-size: 16px; font-weight: bold; color: #fefefe; text-decoration: none; display: inline-block; padding: 8px 16px 8px 16px; border: 0px solid #2199e8; border-radius: 3px; } table.button:hover table tr td a, table.button:active table tr td a, table.button table tr td a:visited, table.button.tiny:hover table tr td a, table.button.tiny:active table tr td a, table.button.tiny table tr td a:visited," +
                " table.button.small:hover table tr td a, table.button.small:active table tr td a, table.button.small table tr td a:visited, table.button.large:hover table tr td a, table.button.large:active table tr td a, table.button.large table tr td a:visited { color: #fefefe; } table.button.tiny table td, table.button.tiny table a { padding: 4px 8px 4px 8px; } table.button.tiny table a { font-size: 10px; font-weight: normal; } table.button.small table td, table.button.small table a { padding: 5px 10px 5px 10px; font-size: 12px; } table.button.large table a { padding: 10px 20px 10px 20px; font-size: 20px; } table.expand, table.expanded " +
                "{ width: 100% !important; } table.expand table, table.expanded table { width: 100%; } table.expand table a, table.expanded table a { width: calc(100% - 20px); text-align: center; } table.expand center, table.expanded center { min-width: 0; } table.button:hover table td, table.button:visited table td, table.button:active table td { background: #147dc2; color: #fefefe; } table.button:hover table a, table.button:visited table a, table.button:active table a { border: 0px solid #147dc2; } table.button.secondary table td { background: #777777; color: #fefefe; border: 2px solid #777777; } table.button.secondary table a {" +
                " color: #fefefe; border: 0px solid #777777; } table.button.secondary:hover table td { background: #919191; color: #fefefe; } table.button.secondary:hover table a { border: 0px solid #919191; } table.button.secondary:hover table td a { color: #fefefe; } table.button.secondary:active table td a { color: #fefefe; } table.button.secondary table td a:visited { color: #fefefe; } table.button.success table td { background: #3adb76; border: 2px solid #3adb76; } table.button.success table a { border: 0px solid #3adb76; } table.button.success:hover table td { background: #23bf5d; } table.button.success:hover table a { border: 0px solid #23bf5d;" +
                " } table.button.alert table td { background: #ec5840; border: 2px solid #ec5840; } table.button.alert table a { border: 0px solid #ec5840; } table.button.alert:hover table td { background: #e23317; } table.button.alert:hover table a { border: 0px solid #e23317; } table.callout { margin-bottom: 16px; Margin-bottom: 16px; } th.callout-inner { width: 100%; border: 1px solid #cbcbcb; padding: 10px; background: #fefefe; } th.callout-inner.primary { background: #def0fc; border: 1px solid #444444; color: #0a0a0a; } th.callout-inner.secondary { background: #ebebeb; border: 1px solid #444444; color: #0a0a0a; } th.callout-inner.success { background:" +
                " #e1faea; border: 1px solid #1b9448; color: #fefefe; } th.callout-inner.warning { background: #fff3d9; border: 1px solid #996800; color: #fefefe; } th.callout-inner.alert { background: #fce6e2; border: 1px solid #b42912; color: #fefefe; } .thumbnail { border: solid 4px #fefefe; box-shadow: 0 0 0 1px rgba(10, 10, 10, 0.2); display: inline-block; line-height: 0; max-width: 100%; transition: box-shadow 200ms ease-out; border-radius: 3px; margin-bottom: 16px; } .thumbnail:hover, .thumbnail:focus { box-shadow: 0 0 6px 1px rgba(33, 153, 232, 0.5); } table.menu { width: 580px; } table.menu td.menu-item, table.menu th.menu-item { padding: 10px; padding-right:" +
                " 10px; } table.menu td.menu-item a, table.menu th.menu-item a { color: #2199e8; } table.menu.vertical td.menu-item, table.menu.vertical th.menu-item { padding: 10px; padding-right: 0; display: block; } table.menu.vertical td.menu-item a, table.menu.vertical th.menu-item a { width: 100%; } table.menu.vertical td.menu-item table.menu.vertical td.menu-item, table.menu.vertical td.menu-item table.menu.vertical th.menu-item, table.menu.vertical th.menu-item table.menu.vertical td.menu-item, table.menu.vertical th.menu-item table.menu.vertical th.menu-item { padding-left: 10px; } table.menu.text-center a { text-align: center; } body.outlook p { display: inline !important;" +
                " } @media only screen and (max-width: 596px) { table.body img { width: auto !important; height: auto !important; } table.body center { min-width: 0 !important; } table.body .container { width: 95% !important; } table.body .columns, table.body .column { height: auto !important; -moz-box-sizing: border-box; -webkit-box-sizing: border-box; box-sizing: border-box; padding-left: 16px !important; padding-right: 16px !important; } table.body .columns .column, table.body .columns .columns, table.body .column .column, table.body .column .columns { padding-left: 0 !important; padding-right: 0 !important; } table.body .collapse .columns, table.body .collapse .column { padding-left:" +
                " 0 !important; padding-right: 0 !important; } td.small-1, th.small-1 { display: inline-block !important; width: 8.33333% !important; } td.small-2, th.small-2 { display: inline-block !important; width: 16.66667% !important; } td.small-3, th.small-3 { display: inline-block !important; width: 25% !important; } td.small-4, th.small-4 { display: inline-block !important; width: 33.33333% !important; } td.small-5, th.small-5 { display: inline-block !important; width: 41.66667% !important; } td.small-6, th.small-6 { display: inline-block !important; width: 50% !important; } td.small-7, th.small-7 { display: inline-block !important; width: 58.33333% !important; } td.small-8, th.small-8" +
                " { display: inline-block !important; width: 66.66667% !important; } td.small-9, th.small-9 { display: inline-block !important; width: 75% !important; } td.small-10, th.small-10 { display: inline-block !important; width: 83.33333% !important; } td.small-11, th.small-11 { display: inline-block !important; width: 91.66667% !important; } td.small-12, th.small-12 { display: inline-block !important; width: 100% !important; } .columns td.small-12, .column td.small-12, .columns th.small-12, .column th.small-12 { display: block !important; width: 100% !important; } .body .columns td.small-1, .body .column td.small-1, td.small-1 center, .body .columns th.small-1, .body .column th.small-1, th.small-1 center" +
                " { display: inline-block !important; width: 8.33333% !important; } .body .columns td.small-2, .body .column td.small-2, td.small-2 center, .body .columns th.small-2, .body .column th.small-2, th.small-2 center { display: inline-block !important; width: 16.66667% !important; } .body .columns td.small-3, .body .column td.small-3, td.small-3 center, .body .columns th.small-3, .body .column th.small-3, th.small-3 center { display: inline-block !important; width: 25% !important; } .body .columns td.small-4, .body .column td.small-4, td.small-4 center, .body .columns th.small-4, .body .column th.small-4, th.small-4 center { display: inline-block !important; width: 33.33333% !important; }" +
                " .body .columns td.small-5, .body .column td.small-5, td.small-5 center, .body .columns th.small-5, .body .column th.small-5, th.small-5 center { display: inline-block !important; width: 41.66667% !important; } .body .columns td.small-6, .body .column td.small-6, td.small-6 center, .body .columns th.small-6, .body .column th.small-6, th.small-6 center { display: inline-block !important; width: 50% !important; } .body .columns td.small-7, .body .column td.small-7, td.small-7 center, .body .columns th.small-7, .body .column th.small-7, th.small-7 center { display: inline-block !important; width: 58.33333% !important; } .body .columns td.small-8, .body .column td.small-8, td.small-8 center," +
                " .body .columns th.small-8, .body .column th.small-8, th.small-8 center { display: inline-block !important; width: 66.66667% !important; } .body .columns td.small-9, .body .column td.small-9, td.small-9 center, .body .columns th.small-9, .body .column th.small-9, th.small-9 center { display: inline-block !important; width: 75% !important; } .body .columns td.small-10, .body .column td.small-10, td.small-10 center, .body .columns th.small-10, .body .column th.small-10, th.small-10 center { display: inline-block !important; width: 83.33333% !important; } .body .columns td.small-11, .body .column td.small-11, td.small-11 center, .body .columns th.small-11, .body .column th.small-11," +
                " th.small-11 center { display: inline-block !important; width: 91.66667% !important; } table.body td.small-offset-1, table.body th.small-offset-1 { margin-left: 8.33333% !important; Margin-left: 8.33333% !important; } table.body td.small-offset-2, table.body th.small-offset-2 { margin-left: 16.66667% !important; Margin-left: 16.66667% !important; } table.body td.small-offset-3, table.body th.small-offset-3 { margin-left: 25% !important; Margin-left: 25% !important; } table.body td.small-offset-4, table.body th.small-offset-4 { margin-left: 33.33333% !important; Margin-left: 33.33333% !important; } table.body td.small-offset-5, table.body th.small-offset-5 { margin-left: 41.66667% !important;" +
                " Margin-left: 41.66667% !important; } table.body td.small-offset-6, table.body th.small-offset-6 { margin-left: 50% !important; Margin-left: 50% !important; } table.body td.small-offset-7, table.body th.small-offset-7 { margin-left: 58.33333% !important; Margin-left: 58.33333% !important; } table.body td.small-offset-8, table.body th.small-offset-8 { margin-left: 66.66667% !important; Margin-left: 66.66667% !important; } table.body td.small-offset-9, table.body th.small-offset-9 { margin-left: 75% !important; Margin-left: 75% !important; } table.body td.small-offset-10, table.body th.small-offset-10 { margin-left: 83.33333% !important; Margin-left: 83.33333% !important; " +
                "} table.body td.small-offset-11, table.body th.small-offset-11 { margin-left: 91.66667% !important; Margin-left: 91.66667% !important; } table.body table.columns td.expander, table.body table.columns th.expander { display: none !important; } table.body .right-text-pad, table.body .text-pad-right { padding-left: 10px !important; } table.body .left-text-pad, table.body .text-pad-left { padding-right: 10px !important; } table.menu { width: 100% !important; } table.menu td, table.menu th { width: auto !important; display: inline-block !important; } table.menu.vertical td, table.menu.vertical th, table.menu.small-vertical td, table.menu.small-vertical th { display: block !important;" +
                " } table.button.expand { width: 100% !important; } } </style> <style> .header { background: #002d30; } .header p { color: #ffffff; margin: 0; } .header .columns { padding-bottom: 0; } .header .container { background: #002d30; padding-top: 20px; padding-bottom: 16px; } .transparent {	 background: #14b9ee;	 padding: 30px 0;	 color: white !important; } .transparent p {	 color: white !important; } .header .container td { padding-top: 20px; padding-bottom: 16px; } </style> </head> <body> <!-- <style> --> <table class='body' data-made-with-foundation=''> <tr> <td class='center' align='center' valign='top'> <center data-parsed=''> <div class='header text-center' align='center'>" +
                " <table class='container'> <tbody> <tr> <td> <table class='row collapse'> <tbody> <tr> <th class='small-6 large-6 columns first'> <table> <tr> <th> <img src='http://mileeyes.com/img/mile_eyes.png' height='40' width='156'> </th> </tr> </table> </th> <th class='small-6 large-6 columns last'> <table> <tr> <th> <p class='text-right'>" +
                company.Name +
                " - Driver Invite!</p> </th> </tr> </table> </th> </tr> </tbody> </table> </td> </tr> </tbody> </table> </div> <table class='container text-center'> <tbody> <tr> <td> <table class='row'> <tbody> <th class='small-12 large-12 columns first last'> <table>	 <tr> <th> <br> <h1>Hello,</h1> <p class='lead'>You've been added as a driver for " +
                company.Name +
                ".</p> <br/> <p>You'll now be able to track new journeys for, and submit mileage expenses to " +
                company.Name + ". Simply choose " + company.Name +
                " from the list of companies at the end of your journey and we'll do the rest. </p> <p>What's more, it won't cost you anything more to track mileage for " +
                company.Name +
                                   ".</p> <table class='callout'> <tr> <td class='callout-inner primary button-row'> <br/> <a href='http://localhost:4000/#/invite?type=driver&invite=" + invite.Id + "&email=" + email + "&companyId=" + companyId + "&companyName=" + company.Name + "' class='button'>Join</a> </td> <td class='expander'></td> </tr>								</table> </th> </tr> </table> </th> </tr> </tbody> </table> </td> </tr> <tr class='transparent'>				 <th>					 <br/>					 <br/>					 <p>&copy; Powerhouse Software</p>					 <br/>					 <br/>				 </th>			 </tr> </tbody> </table> </center> </td> </tr> </table> </body></html>");

            return Ok("Email has been sent!");
        }

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

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

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