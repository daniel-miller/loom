using System;
using System.Web;

namespace Loom
{
    /// <summary>
    /// Reads the tenant slug and settings that <see cref="OrganizationResolver"/>
    /// stored in <c>HttpContext.Items</c> during <c>Application_BeginRequest</c>.
    /// Does not touch <see cref="OrganizationCache"/> directly, so a concurrent
    /// cache reload cannot affect an in-flight request.
    /// </summary>
    public class WebOrganizationContext : IOrganizationContext
    {
        private readonly HttpContextBase _context;

        public WebOrganizationContext(HttpContextBase context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string Slug => _context.Items[OrganizationResolver.SlugItemKey] as string;

        public OrganizationSettings Settings
        {
            get
            {
                if (_context.Items[OrganizationResolver.SettingsItemKey] is OrganizationSettings settings)
                    return settings;

                throw new InvalidOperationException(
                    "Organization settings are not in HttpContext.Items. " +
                    "OrganizationResolver.Resolve must run during Application_BeginRequest before pages read tenant settings.");
            }
        }
    }
}
