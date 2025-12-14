using System;

namespace Loom
{
    public partial class ContextInvalid : System.Web.UI.Page
    {
        protected string InvalidOrganizationSlug { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            InvalidOrganizationSlug = Request.QueryString["requested"] ?? "(unknown)";
        }
    }
}