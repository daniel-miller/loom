using System;
using System.Web.UI;

namespace Loom
{
    /// <summary>
    /// App-scope about page served at <c>/about</c>. Renders without a tenant context.
    /// Tenant about lives at <c>/{slug}/about</c> and is handled by <see cref="Loom.Tenants.About"/>.
    /// </summary>
    public partial class About : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}
