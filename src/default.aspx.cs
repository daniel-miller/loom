using System;
using System.Configuration;
using System.Web.UI;

namespace Loom
{
    public partial class Default : Page
    {
        private IOrganizationContext _orgContext = new WebOrganizationContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            MainHeading.InnerHtml = "Welcome to the " + OrganizationHtml.ColoredName(_orgContext.Settings);

            var scheme = Request.Url.Scheme;

            var subdomain = "indigo.";

            if (Request.IsLocal)
                subdomain = "local-" + subdomain;

            var domain = ConfigurationManager.AppSettings["Loom.RemoteDomain"];

            IndigoAnchor.HRef = $"{scheme}://{subdomain}{domain}";
        }
    }
}