using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PeriodicTable.Startup))]
namespace PeriodicTable
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
