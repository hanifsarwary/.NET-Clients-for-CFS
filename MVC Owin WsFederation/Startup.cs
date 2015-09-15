using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.WsFederation;
using MVC_Owin_WsFederation;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace MVC_Owin_WsFederation
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            app.UseWsFederationAuthentication(new WsFederationAuthenticationOptions
            {
                MetadataAddress = MVC_Owin_WsFederation.Configuration.CFS_METADATA,
                Wtrealm = MVC_Owin_WsFederation.Configuration.CFS_RP_REALM,

                SignInAsAuthenticationType = "Cookies"
            });
        }
    }
}
