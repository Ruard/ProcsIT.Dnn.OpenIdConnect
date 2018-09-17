﻿using System;
using System.Collections.Specialized;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Authentication.Oidc;

namespace ProcsIT.Dnn.AuthServices.OpenIdConnect
{
    public abstract class OidcLoginBase : AuthenticationLoginBase
    {
        protected virtual string AuthSystemApplicationName
        {
            get { return String.Empty; }
        }

        protected OidcClientBase OAuthClient { get; set; }

        protected abstract UserData GetCurrentUser();

        protected virtual void AddCustomProperties(NameValueCollection properties)
        {
            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                //Save the return Url in the cookie
                HttpContext.Current.Response.Cookies.Set(new HttpCookie("returnurl", RedirectURL)
                {
                    Expires = DateTime.Now.AddMinutes(5),
                    Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/")
                });
            }

            bool shouldAuthorize = OAuthClient.IsCurrentService() && OAuthClient.HaveVerificationCode();
            if(Mode == AuthMode.Login)
            {
                shouldAuthorize = shouldAuthorize || OAuthClient.IsCurrentUserAuthorized();
            }

            if (shouldAuthorize)
            {
                if (OAuthClient.Authorize() == AuthorisationResult.Authorized)
                {
                    OAuthClient.AuthenticateUser(GetCurrentUser(), PortalSettings, IPAddress, AddCustomProperties, OnUserAuthenticated);
                }
            }
        }

        public override bool Enabled
        {
            get { return OidcConfigBase.GetConfig(AuthSystemApplicationName, PortalId).Enabled; }
        }

    }
}