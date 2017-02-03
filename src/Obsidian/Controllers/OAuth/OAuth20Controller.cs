﻿using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Obsidian.Application.OAuth20;
using Obsidian.Application.ProcessManagement;
using Obsidian.Domain;
using Obsidian.Misc;
using Obsidian.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Obsidian.Application.Authentication;

#pragma warning disable CS1591
namespace Obsidian.Controllers.OAuth
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class OAuth20Controller : Controller
    {
        private readonly IDataProtector _dataProtector;
        private readonly SagaBus _sagaBus;

        public OAuth20Controller(IDataProtectionProvider dataProtectionProvicer, SagaBus bus)
        {
            _dataProtector = dataProtectionProvicer.CreateProtector("Obsidian.OAuth.Context.Key");
            _sagaBus = bus;
        }


        [HttpGet("oauth20/authorize")]
        [ValidateModel]
        [Authorize(ActiveAuthenticationSchemes = "Obsidian.Cookie")]
        [AllowAnonymous]
        public async Task<IActionResult> Authorize([FromQuery]AuthorizationRequestModel model)
        {
            AuthorizationGrant grantType;
            try
            {
                grantType = ParseGrantType(model.ResponseType);
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest();
            }
            var command = new AuthorizeCommand
            {
                ClientId = model.ClientId,
                ScopeNames = model.Scope.Split(' '),
                GrantType = grantType
            };

            if (User.Identity.IsAuthenticated)
            {
                command.UserName = User.Identity.Name;
            }

            var result = await _sagaBus.InvokeAsync<AuthorizeCommand, OAuth20Result>(command);
            var context = _dataProtector.Protect(result.SagaId.ToString());
            switch (result.State)
            {
                case OAuth20State.RequireSignIn:
                    return View("SignIn", new OAuthSignInModel { ProtectedOAuthContext = context });

                case OAuth20State.RequirePermissionGrant:
                    return PermissionGrantView(result);

                case OAuth20State.AuthorizationCodeGenerated:
                    return AuthorizationCodeRedirect(result);

                case OAuth20State.Finished:
                    return ImplictRedirect(result);

                default:
                    return BadRequest();
            }
        }

        [HttpPost("oauth20/authorize")]
        [ValidateModel]
        public async Task<IActionResult> SignIn([FromForm]OAuthSignInModel model)
        {
            Guid sagaId;
            var context = _dataProtector.Unprotect(model.ProtectedOAuthContext);
            if (!Guid.TryParse(context, out sagaId))
            {
                return BadRequest();
            }
            var command = new PasswordSignInCommand
            {
                UserName = model.UserName,
                Password = model.Password,
                IsPresistent = model.RememberMe
            };
            var authResult = await _sagaBus.InvokeAsync<PasswordSignInCommand, AuthenticationResult>(command);
            if (!authResult.IsCredentialVaild)
            {
                ModelState.AddModelError(string.Empty, "Singin failed");
                    return View("SignIn");
            }
            var message = new OAuth20SignInMessage(sagaId)
            {
                UserName = model.UserName,
            };
            var oauth20Result = await _sagaBus.SendAsync<OAuth20SignInMessage, OAuth20Result>(message);
            switch (oauth20Result.State)
            {
                case OAuth20State.RequirePermissionGrant:
                    return PermissionGrantView(oauth20Result);

                case OAuth20State.AuthorizationCodeGenerated:
                    return AuthorizationCodeRedirect(oauth20Result);

                case OAuth20State.Finished:
                    return ImplictRedirect(oauth20Result);

                default:
                    return BadRequest();
            }
        }

        [HttpPost("oauth20/authorize/permission")]
        [ValidateModel]
        public async Task<IActionResult> PermissionGrant([FromForm]PermissionGrantModel model)
        {
            Guid sagaId;
            var context = _dataProtector.Unprotect(model.ProtectedOAuthContext);
            if (!Guid.TryParse(context, out sagaId))
            {
                return BadRequest();
            }
            var message = new PermissionGrantMessage(sagaId)
            {
                GrantedScopeNames = model.GrantedScopeNames ?? new List<string>()
            };
            var result = await _sagaBus.SendAsync<PermissionGrantMessage, OAuth20Result>(message);
            switch (result.State)
            {
                case OAuth20State.AuthorizationCodeGenerated:
                    return AuthorizationCodeRedirect(result);

                case OAuth20State.Finished:
                    return ImplictRedirect(result);

                case OAuth20State.UserDenied:
                    return View("UserDenied");

                default:
                    return BadRequest();
            }
        }

        [HttpPost("oauth20/token")]
        [ValidateModel]
        public async Task<IActionResult> Token([FromBody]AuthorizationCodeGrantRequestModel model)
        {
            if ("authorization_code".Equals(model.GrantType, StringComparison.OrdinalIgnoreCase))
            {
                var message = new AccessTokenRequestMessage(model.Code)
                {
                    ClientId = model.ClientId,
                    ClientSecret = model.ClientSecret,
                    Code = model.Code
                };
                var result = await _sagaBus.SendAsync<AccessTokenRequestMessage, OAuth20Result>(message);
                switch (result.State)
                {
                    case OAuth20State.AuthorizationCodeGenerated:
                        return BadRequest();

                    case OAuth20State.Finished:
                        return Ok(TokenResponseModel.FromOAuth20Result(result));
                }
            }
            return BadRequest();
        }


        private AuthorizationGrant ParseGrantType(string responseType)
        {
            if ("code".Equals(responseType, StringComparison.OrdinalIgnoreCase))
            {
                return AuthorizationGrant.AuthorizationCode;
            }
            else if ("token".Equals(responseType, StringComparison.OrdinalIgnoreCase))
            {
                return AuthorizationGrant.Implicit;
            }
            else
                throw new ArgumentOutOfRangeException(nameof(responseType),
                            "Only code and token can be accepted as response type.");
        }


        private static string BuildImplictReturnUri(OAuth20Result result)
        {
            var sb = new StringBuilder($"{result.RedirectUri}?access_token={result.Token.AccessToken}");
            if (result.Token.AuthrneticationToken != null)
            {
                sb.Append($"&authentication_token={result.Token.AuthrneticationToken}");
            }
            if (result.Token.RefreshToken != null)
            {
                sb.Append($"&refresh_token={result.Token.RefreshToken}");
            }
            var tokenRedirectUri = sb.ToString();
            return tokenRedirectUri;
        }

        #region Results

        private IActionResult PermissionGrantView(OAuth20Result result)
        {
            var context = _dataProtector.Protect(result.SagaId.ToString());
            ViewBag.Client = result.PermissionGrant.Client;
            ViewBag.Scopes = result.PermissionGrant.Scopes;
            return View("PermissionGrant", new PermissionGrantModel { ProtectedOAuthContext = context });
        }

        private IActionResult ImplictRedirect(OAuth20Result result)
        {
            var tokenRedirectUri = BuildImplictReturnUri(result);
            return Redirect(tokenRedirectUri);
        }

        private IActionResult AuthorizationCodeRedirect(OAuth20Result result)
        {
            var codeRedirectUri = $"{result.RedirectUri}?code={result.AuthorizationCode}";
            return Redirect(codeRedirectUri);
        }

        #endregion Results

        #region Front-end debug

        [Route("oauth20/authorize/frontend/signin")]
        [HttpGet]
        public IActionResult FrontEndSignInDebug()
        {
            return View("SignIn");
        }

        [Route("oauth20/authorize/frontend/grant")]
        [HttpGet]
        public IActionResult FrontEndGrantDebug()
        {
            var context = _dataProtector.Protect(Guid.NewGuid().ToString());
            var client = Client.Create(Guid.NewGuid(), "http://za-pt.org/exchange");
            client.DisplayName = "iTech";
            ViewBag.Client = client;
            ViewBag.Scopes = new[] {
                PermissionScope.Create(Guid.NewGuid(),"obsidian.basicinfo","Basic Information","Includes you name and gender."),
                PermissionScope.Create(Guid.NewGuid(),"obsidian.email","Email address","Your email address."),
                PermissionScope.Create(Guid.NewGuid(),"obsidian.admin","Admin access","Manage the system.")
            };
            return View("PermissionGrant", new PermissionGrantModel { ProtectedOAuthContext = context });
        }

        [Route("oauth20/authorize/frontend/denied")]
        [HttpGet]
        public IActionResult FrontEndDeniedDebug()
        {
            return View("UserDenied");
        }

        #endregion Front-end debug
    }
}