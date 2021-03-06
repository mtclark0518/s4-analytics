﻿using Lib.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using S4Analytics.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Hosting;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Runtime.Serialization;

namespace S4Analytics.Controllers
{
    public class Credentials
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    [Route("api/[controller]")]
    [Authorize]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class IdentityController : Controller
    {
        IHostingEnvironment _env;
        SignInManager<S4IdentityUser<S4UserProfile>> _signInManager;
        UserManager<S4IdentityUser<S4UserProfile>> _userManager;
        IdentityOptions _identityOptions;
        Html5Conduit _html5Conduit;

        public IdentityController(
            IHostingEnvironment env,
            SignInManager<S4IdentityUser<S4UserProfile>> signInManager,
            UserManager<S4IdentityUser<S4UserProfile>> userManager,
            IOptions<IdentityOptions> identityOptions,
            Html5Conduit html5Conduit)
        {
            _env = env;
            _signInManager = signInManager;
            _userManager = userManager;
            _identityOptions = identityOptions.Value;
            _html5Conduit = html5Conduit;
        }

        [HttpPost("login/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> LogInWithToken(string token)
        {
            var tokenAsGuid = Guid.Parse(token);
            var details = _html5Conduit.GetDetailsFromToken(tokenAsGuid);
            var user = await _userManager.FindByNameAsync(details.UserName);
            var payloadStream = details.JsonPayload.ToStream();
            var ser = new DataContractJsonSerializer(typeof(UrlPayload));
            var payload = (UrlPayload)ser.ReadObject(payloadStream);

            if (user == null)
            {
                return NotFound();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            return new ObjectResult(new
            {
                // todo: return an S4IdentityUser
                user = new
                {
                    user.UserName,
                    roles = user.Roles.Select(role => role.RoleName)
                },
                payload
            });
        }

        /// <summary>
        /// Log in.
        /// </summary>
        /// <param name="credentials">Username and password</param>
        /// <returns></returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LogIn([FromBody] Credentials credentials)
        {
            var user = await _userManager.FindByNameAsync(credentials.UserName);
            if (user == null) { return Unauthorized(); }

            var persistCookieInBrowser = _env.EnvironmentName == "Local";

            var signInResult = await _signInManager.PasswordSignInAsync(
                user, credentials.Password, persistCookieInBrowser, lockoutOnFailure: false);
            if (signInResult != Microsoft.AspNetCore.Identity.SignInResult.Success)
            {
                // sign out in case they had logged in successfully prior to this failed attempt
                await _signInManager.SignOutAsync();
                return Unauthorized();
            }

            // todo: return an S4IdentityUser
            return new ObjectResult(new {
                user.UserName,
                roles = user.Roles.Select(role => role.RoleName)
            });
        }

        /// <summary>
        /// Log out currently authenticated user.
        /// </summary>
        /// <returns></returns>
        [HttpPost("logout")]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return new ObjectResult(new { success = true });
        }

        [HttpGet("current-user")]
        public IActionResult GetCurrentUser()
        {
            // todo: return an S4IdentityUser
            return new ObjectResult(new {
                userName = _userManager.GetUserName(User),
                roles = GetCurrentUserRoles()
            });
        }

        private IList<string> GetCurrentUserRoles()
        {
            var roles = User.Claims
                .Where(claim => claim.Type == _identityOptions.ClaimsIdentity.RoleClaimType)
                .Select(claim => claim.Value)
                .ToList();
            return roles;
        }
    }

    [DataContract]
    class UrlPayload
    {
        [DataMember]
        public string url;
    }
}
