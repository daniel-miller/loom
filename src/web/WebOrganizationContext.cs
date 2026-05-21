using System.Web;

namespace Loom
{
    public class WebOrganizationContext : IOrganizationContext
    {
        public string Slug => HttpContext.Current.Items[OrganizationResolver.SlugVariable] as string;

        public OrganizationSettings Settings => GetSettings();

        private OrganizationSettings GetSettings()
        {
            if (HttpContext.Current.Items[OrganizationResolver.SettingsVariable] is OrganizationSettings settings)
                return settings;

            settings = OrganizationCache.GetBySlug(Slug);

            HttpContext.Current.Items[OrganizationResolver.SettingsVariable] = settings;

            return settings;
        }
    }
}