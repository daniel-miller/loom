using System;
using System.Web;
using System.Web.Routing;
using Loom.Diagnostics;

namespace Loom
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            OrganizationResolver.EnsureConfigured();

            RouteConfiguration.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            OrganizationResolver.Resolve(new HttpContextWrapper(Context));
        }

        protected void Application_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            var slug = Context.Items[OrganizationResolver.SlugItemKey] as string;

            var isMissingSlug = string.IsNullOrEmpty(slug);

            var isEmptySlug = string.Equals(slug, OrganizationCache.EmptySlug, StringComparison.OrdinalIgnoreCase);

            var isHtml = Response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) == true;

            if (!isMissingSlug && !isEmptySlug && isHtml)
            {
                Response.Filter = new OrganizationUrlResponseFilter(Response.Filter, slug, Response.ContentEncoding);
            }
        }

        void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            if (ex == null) return;

            // HttpUnhandledException wraps the real exception once ASP.NET captures it.
            if (ex is HttpUnhandledException && ex.InnerException != null)
                ex = ex.InnerException;

            var slug = Context.Items[OrganizationResolver.SlugItemKey] as string;
            var path = Request?.Url?.PathAndQuery ?? "(no request url)";

            LoomLog.Current.Error(
                "Unhandled exception. tenant={0} path={1}",
                ex,
                string.IsNullOrEmpty(slug) ? "(none)" : slug,
                path);

            // Let ASP.NET continue to the configured customErrors handler.
        }
    }
}