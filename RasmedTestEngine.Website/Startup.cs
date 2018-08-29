using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RasmedTestEngine.Website.Startup))]
namespace RasmedTestEngine.Website
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
           

            ConfigureAuth(app);


       
        }
    }
}
