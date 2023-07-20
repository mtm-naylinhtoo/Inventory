using Inventory.Mailers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Inventory
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            var mailHogHost = "localhost";
            var mailHogPort = 1025; // The default MailHog SMTP port
            EmailService emailService = new EmailService(mailHogHost, mailHogPort);
            HttpContext.Current.Application["EmailService"] = emailService;
        }
    }
}
