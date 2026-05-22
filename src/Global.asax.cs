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
            var ctx = new HttpContextWrapper(Context);

            RequestId.GetOrCreate(ctx);

            OrganizationResolver.Resolve(ctx);
        }

        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            var id = RequestId.Current;
            if (!string.IsNullOrEmpty(id))
                Response.Headers[RequestId.HeaderName] = id;
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
                "Unhandled exception. requestId={0} tenant={1} path={2}",
                ex,
                RequestId.Current ?? "(none)",
                string.IsNullOrEmpty(slug) ? "(none)" : slug,
                path);

            // Let ASP.NET continue to the configured customErrors handler.
        }
    }
}