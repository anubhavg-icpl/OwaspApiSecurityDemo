using System.Web.Http;
using OwaspApiSecurityDemo.App.Infrastructure;
using Owin;

namespace OwaspApiSecurityDemo.App
{
    public sealed class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never;
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.MessageHandlers.Add(new DemoSecurityHeadersHandler());

            app.UseWebApi(config);
        }
    }
}
