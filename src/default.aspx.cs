using System;
using System.Web.UI;

namespace Loom
{
    /// <summary>
    /// App-scope landing page served at <c>/</c>. Renders without a tenant context.
    /// Tenant home lives at <c>/{slug}</c> and is handled by <see cref="Loom.Tenants.TenantHome"/>.
    /// </summary>
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}
