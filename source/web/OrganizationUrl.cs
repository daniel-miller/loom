using System.Web;

namespace Loom
{
    public static class OrganizationUrl
    {
        /// <summary>
        /// Prepends a root-relative path with the organization slug (i.e., tenant account).
        /// </summary>
        public static string Resolve(string relativePath, string organizationSlug = null)
        {
            var organization = organizationSlug ??
                HttpContext.Current.Items[OrganizationResolver.SlugVariable] as string;

            var cleanPath = relativePath.TrimStart('~', '/');

            return $"/{organization}/{cleanPath}";
        }

        /// <summary>
        /// Returns an absolute URL resolved to the current organization account.
        /// </summary>
        public static string ResolveAbsolute(string relativePath, string organizationSlug = null)
        {
            var request = HttpContext.Current.Request;

            return $"{request.Url.Scheme}://{request.Url.Host}{Resolve(relativePath, organizationSlug)}";
        }
    }
}