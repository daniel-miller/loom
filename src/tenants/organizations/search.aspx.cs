using System;
using System.Web;

namespace Loom.Tenants
{
    public partial class Search : System.Web.UI.Page
    {
        private IOrganizationContext OrganizationContext;

        protected void Page_Load(object sender, EventArgs e)
        {
            OrganizationContext = new WebOrganizationContext(new HttpContextWrapper(Context));

            OrganizationRepeater.DataSource = OrganizationCache.GetAll();
            OrganizationRepeater.DataBind();
        }

        protected bool IsCurrentOrganization(string slug)
        {
            return string.Equals(slug, OrganizationContext.Slug, StringComparison.OrdinalIgnoreCase);
        }
    }
}
