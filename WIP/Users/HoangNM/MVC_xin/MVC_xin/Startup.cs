using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVC_xin.Startup))]
namespace MVC_xin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
