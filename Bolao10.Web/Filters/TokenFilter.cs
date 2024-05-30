using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bolao10.Persistence.Repository;
using Bolao10.Model.Entities;
using Bolao10.Services;
using System.Collections.Specialized;
using System.Web.Routing;
using System.Net.Http;
using System.Web.Security;

namespace Bolao10.Web.Filters
{
    public class TokenFilter : ActionFilterAttribute
    {

        public TokenFilter()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var token1 = filterContext.HttpContext.Session["token1"];

            if (token1 == null)
            {
                //FormsAuthentication.SignOut();

                //filterContext.HttpContext.Session.Abandon();

                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary { { "controller", "Account" }, { "action", "Login" } });

            }


        }

 
    }
}