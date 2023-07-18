using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers
{
    public class UserAuthorizationFilter : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // Check if the current user exists in the session (logged in)
            if (filterContext.HttpContext.Session["CurrentUser"] == null)
            {
                // Redirect to the login page or any other unauthorized page
                filterContext.Result = new RedirectResult("/Users/Login");
            }
        }
    }
}