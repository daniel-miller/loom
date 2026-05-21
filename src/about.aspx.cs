using System;
using System.Web;
using System.Web.UI;

namespace Loom
{
    public partial class About : Page
    {
        private IOrganizationContext _orgContext;

        protected void Page_Load(object sender, EventArgs e)
        {
            _orgContext = new WebOrganizationContext(new HttpContextWrapper(Context));

            MainHeading.InnerHtml = "About the " + OrganizationHtml.ColoredName(_orgContext.Settings);
        }
    }
}