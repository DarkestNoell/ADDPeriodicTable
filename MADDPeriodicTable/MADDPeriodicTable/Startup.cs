using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MADDPeriodicTable.Startup))]
namespace MADDPeriodicTable
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
