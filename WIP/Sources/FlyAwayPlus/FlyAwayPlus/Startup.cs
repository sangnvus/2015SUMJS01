using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FlyAwayPlus.Startup))]
namespace FlyAwayPlus
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
