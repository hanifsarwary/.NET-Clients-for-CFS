using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MVC_OpenID_Connect.Models;

namespace MVC_OpenID_Connect
{
    public class Global : HttpApplication
    {
        public static OpenIdConnectClient OpenIdConnectClient { get; set; }
        
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            string appId = ConfigurationManager.AppSettings["ApplicationId"];
            string appSecret = ConfigurationManager.AppSettings["ApplicationSecret"];
            string authorizationEndpoint = ConfigurationManager.AppSettings["AuthorizationEndpoint"];
            string tokenEndpoint = ConfigurationManager.AppSettings["TokenEndpoint"];
            string resourceEndpoint = ConfigurationManager.AppSettings["ResourceEndpoint"];
            string scope = ConfigurationManager.AppSettings["Scope"];

            OpenIdConnectClient = new OpenIdConnectClient(appId)
            {
                ApplicationSecret = appSecret,
                AuthorizationEndpoint = authorizationEndpoint,
                TokenEndpoint = tokenEndpoint,
                ResourceEndpoint = resourceEndpoint,
                Scope = scope
            };
        }
    }
}