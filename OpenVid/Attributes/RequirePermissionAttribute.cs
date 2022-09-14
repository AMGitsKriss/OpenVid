using Database.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;

namespace OrionDashboard.Web.Attributes
{/*
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(Permissions permission) : base(typeof(RequirePermissionFilter))
        {

        }
    }*/

    public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        private Permissions _permission;

        public RequirePermissionAttribute(Permissions permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userRepository = context.HttpContext.RequestServices.GetService(typeof(UserRepository)) as UserRepository;
            var configuration = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;

            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            bool.TryParse(configuration["Authentication:RequireLogin"], out bool requireLogin);
            bool.TryParse(configuration["Authentication:RequirePermission"], out bool requirePermission);

            if (!requireLogin || !requirePermission)
                return;

            if (userId == null)
            {
                context.Result = new RedirectResult("/Login");
                return;
            }

            var hasPerm = userRepository.UserHasPermission(userId.Value, _permission);

            if (!hasPerm)
                context.Result = new StatusCodeResult(401);
        }
    }

}
