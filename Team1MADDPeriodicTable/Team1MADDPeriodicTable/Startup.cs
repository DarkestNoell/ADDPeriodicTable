using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Team1MADDPeriodicTable.Startup))]
namespace Team1MADDPeriodicTable
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
