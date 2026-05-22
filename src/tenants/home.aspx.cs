using System;
using System.Configuration;

namespace Loom.Tenants
{
    public partial class TenantHome : TenantPage
    {
        private const string DemoSubdomainSlugKey = "Loom.Demo.SubdomainOrganizationSlug";

        protected void Page_Load(object sender, EventArgs e)
        {
            MainHeading.InnerHtml = "Welcome to the " + OrganizationHtml.ColoredName(OrgContext.Settings);

            ConfigureSubdomainDemoLink();
        }

        private void ConfigureSubdomainDemoLink()
        {
            var demoSlug = ConfigurationManager.AppSettings[DemoSubdomainSlugKey];

            if (string.IsNullOrWhiteSpace(demoSlug) || !OrganizationCache.IsValidOrganization(demoSlug))
            {
                IndigoAnchor.Visible = false;
                return;
            }

            var subdomain = Request.IsLocal ? $"local-{demoSlug}." : $"{demoSlug}.";

            var domain = ConfigurationManager.AppSettings["Loom.RemoteDomain"];

            IndigoAnchor.HRef = $"{Request.Url.Scheme}://{subdomain}{domain}";
            IndigoAnchor.InnerText = $"Navigate to the {char.ToUpperInvariant(demoSlug[0]) + demoSlug.Substring(1)} organization (absolute URL)";
        }
    }
}
