using System;

namespace Loom.Tenants
{
    public partial class TenantHome : TenantPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            MainHeading.InnerHtml = "Welcome to the " + OrganizationHtml.ColoredName(OrgContext.Settings);
        }
    }
}
