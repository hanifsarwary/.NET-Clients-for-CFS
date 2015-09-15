using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVC_Owin_WsFederation.Startup))]
namespace MVC_Owin_WsFederation
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
