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
            var name = _orgContext.Settings.Name;

            var color = _orgContext.Settings.Color;

            var span = $"<span style='color:{color}'>{name}</span>";

            MainHeading.InnerHtml = "Welcome to the " + span;

            var scheme = Request.Url.Scheme;

            var subdomain = "indigo.";

            if (Request.IsLocal)
                subdomain = "local-" + subdomain;

            var domain = ConfigurationManager.AppSettings["Loom.RemoteDomain"];

            IndigoAnchor.HRef = $"{scheme}://{subdomain}{domain}";
        }
    }
}