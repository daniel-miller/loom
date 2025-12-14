using System;
using System.Web.UI;

namespace Loom
{
    public partial class About : Page
    {
        private IOrganizationContext _orgContext = new WebOrganizationContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            var name = _orgContext.Settings.Name;

            var color = _orgContext.Settings.Color;

            var span = $"<span style='color:{color}'>{name}</span>";

            MainHeading.InnerHtml = "About the " + span;
        }
    }
}