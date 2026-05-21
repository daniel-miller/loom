using System;
using System.Web.UI;

namespace Loom
{
    public partial class About : Page
    {
        private IOrganizationContext _orgContext = new WebOrganizationContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            MainHeading.InnerHtml = "About the " + OrganizationHtml.ColoredName(_orgContext.Settings);
        }
    }
}