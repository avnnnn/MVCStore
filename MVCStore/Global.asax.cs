using MVCStore.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MVCStore
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_AuthenticateRequest()
        {
            //check is user log in
            if (User == null)
                return;
            //get user name
            string userName = Context.User.Identity.Name;
            //declare roles array
            string[] roles = null;
            using (Db db = new Db())
            {
                //fill  roles
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == userName);
                if (dto == null)
                    return;
                roles = db.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray();
            }
            //create object interface IPrincipal
            IIdentity userIdentity = new GenericIdentity(userName);
            IPrincipal newUserObj = new GenericPrincipal(userIdentity,roles);

            //declare and init Context.User

            Context.User = newUserObj;
        }
    }
}
