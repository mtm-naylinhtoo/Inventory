using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Inventory.Models;

namespace Inventory.Controllers
{
    public class UserAuthorizationFilter : AuthorizeAttribute
    {
        public bool CheckUserRole { get; set; } = true;
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // Check if the current user exists in the session (logged in)
            if (filterContext.HttpContext.Session["CurrentUser"] == null)
            {
                // Redirect to the login page or any other unauthorized page
                filterContext.Result = new RedirectResult("/Users/Login");
            }

            if (CheckUserRole)
            {
                var user = (User)filterContext.HttpContext.Session["CurrentUser"];
                var role = user.Role;
                if (role != 0)
                {
                    filterContext.Result = new RedirectResult("/Home/Index");
                    return;
                }

            }
        }
    }
}