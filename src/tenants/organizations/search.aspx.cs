using System;

namespace Loom.Tenants
{
    public partial class Search : TenantPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            OrganizationRepeater.DataSource = OrganizationCache.GetAll();
            OrganizationRepeater.DataBind();
        }

        protected bool IsCurrentOrganization(string slug)
        {
            return string.Equals(slug, OrgContext.Slug, StringComparison.OrdinalIgnoreCase);
        }
    }
}
