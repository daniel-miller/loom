using System;
using System.Web;
using System.Web.Routing;

namespace Loom
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            RouteConfiguration.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"BeginRequest: {Request.Url.AbsolutePath}");

            OrganizationResolver.Resolve(Request, Response);
        }

        protected void Application_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            var slug = HttpContext.Current.Items[OrganizationResolver.SlugVariable] as string;

            var isMissingSlug = string.IsNullOrEmpty(slug);

            var isHtml = Response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) == true;

            if (!isMissingSlug && isHtml)
            {
                Response.Filter = new OrganizationUrlResponseFilter(Response.Filter, slug);
            }
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started

        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }
    }
}