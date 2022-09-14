using Database.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;

namespace OrionDashboard.Web.Attributes
{
    public class RequireLoginAttribute : Attribute, IAuthorizationFilter
    {
        private bool _forceLogin;

        public RequireLoginAttribute(bool forceLogin = false)
        {
            _forceLogin = forceLogin;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            var configuration = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;

            bool.TryParse(configuration["Authentication:RequireLogin"], out bool requireLogin);

            if (!_forceLogin && !requireLogin)
                return;

            if (userId == null)
            {
                context.Result = new RedirectResult("/Login");
            }
        }
    }
}
