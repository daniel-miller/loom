using System.Web.Routing;

using Microsoft.AspNet.FriendlyUrls;

namespace Loom
{
    public static class RouteConfiguration
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            var settings = new FriendlyUrlSettings
            {
                AutoRedirectMode = RedirectMode.Permanent
            };

            routes.EnableFriendlyUrls(settings);
        }
    }
}