using System;
using System.Web;

namespace Loom
{
    public class WebOrganizationContext : IOrganizationContext
    {
        private readonly HttpContextBase _context;

        public WebOrganizationContext(HttpContextBase context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string Slug => _context.Items[OrganizationResolver.SlugItemKey] as string;

        public OrganizationSettings Settings => GetSettings();

        private OrganizationSettings GetSettings()
        {
            if (_context.Items[OrganizationResolver.SettingsItemKey] is OrganizationSettings settings)
                return settings;

            settings = OrganizationCache.GetBySlug(Slug);

            _context.Items[OrganizationResolver.SettingsItemKey] = settings;

            return settings;
        }
    }
}
