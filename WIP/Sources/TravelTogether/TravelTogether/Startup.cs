using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TravelTogether.Startup))]
namespace TravelTogether
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
