using System;
using System.Web;

namespace Loom
{
    public static class OrganizationUrl
    {
        /// <summary>
        /// Prepends a root-relative path with the organization slug (i.e., tenant account).
        /// Pass <paramref name="organizationSlug"/> to force a tenant; otherwise the slug
        /// is read from <paramref name="context"/>.Items.
        /// </summary>
        public static string Resolve(HttpContextBase context, string relativePath, string organizationSlug = null)
        {
            if (relativePath == null) throw new ArgumentNullException(nameof(relativePath));

            var organization = organizationSlug ?? ReadSlug(context);

            var cleanPath = relativePath.TrimStart('~', '/');

            return $"/{organization}/{cleanPath}";
        }

        /// <summary>
        /// Returns an absolute URL resolved to the current organization account.
        /// </summary>
        public static string ResolveAbsolute(HttpContextBase context, string relativePath, string organizationSlug = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var request = context.Request;

            return $"{request.Url.Scheme}://{request.Url.Host}{Resolve(context, relativePath, organizationSlug)}";
        }

        private static string ReadSlug(HttpContextBase context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context),
                    "An HttpContextBase is required when no organization slug is supplied explicitly.");

            return context.Items[OrganizationResolver.SlugItemKey] as string;
        }
    }
}
