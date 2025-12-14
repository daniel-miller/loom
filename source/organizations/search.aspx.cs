using System;

namespace Loom.Organizations
{
    public partial class Search : System.Web.UI.Page
    {
        private WebOrganizationContext OrganizationContext = new WebOrganizationContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            OrganizationRepeater.DataSource = OrganizationCache.GetAll();
            OrganizationRepeater.DataBind();
        }

        protected bool IsCurrentOrganization(string slug)
        {
            return string.Equals(slug, OrganizationContext.Slug, StringComparison.OrdinalIgnoreCase);
        }
    }
}