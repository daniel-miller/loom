using System;

namespace Loom.Tenants
{
    public partial class About : TenantPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            MainHeading.InnerHtml = "About the " + OrganizationHtml.ColoredName(OrgContext.Settings);
        }
    }
}
