using System;
using System.Web;
using System.Web.UI;

namespace Loom.Tenants
{
    /// <summary>
    /// Base class for tenant-scoped pages. Wires the per-request organization
    /// context in <see cref="OnInit"/> so derived pages can read
    /// <see cref="OrgContext"/> without repeating the construction boilerplate.
    ///
    /// Tenant pages are reached only via <c>/{slug}/...</c> URLs.
    /// <see cref="OrganizationResolver"/> must have stored the slug and settings
    /// in <c>HttpContext.Items</c> earlier in the request pipeline; reading
    /// <see cref="IOrganizationContext.Settings"/> from an unscoped request will
    /// throw.
    /// </summary>
    public abstract class TenantPage : Page
    {
        protected IOrganizationContext OrgContext { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            OrgContext = new WebOrganizationContext(new HttpContextWrapper(Context));
        }
    }
}
