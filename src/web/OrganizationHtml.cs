using System.Text.RegularExpressions;
using System.Web;

namespace Loom
{
    /// <summary>
    /// Helpers for rendering organization-related HTML safely.
    /// </summary>
    public static class OrganizationHtml
    {
        // Allow CSS color keywords (e.g., "orange") and hex (#abc, #aabbcc, #aabbccdd).
        private static readonly Regex SafeColorPattern = new Regex(
            @"^([a-zA-Z]{1,32}|#[0-9a-fA-F]{3,8})$",
            RegexOptions.Compiled);

        /// <summary>
        /// Returns an HTML span containing the organization name styled with its color.
        /// The name is HTML-encoded; the color is whitelisted and falls back to
        /// <c>inherit</c> if it does not match a safe pattern.
        /// </summary>
        public static string ColoredName(OrganizationSettings settings)
        {
            var safeName = HttpUtility.HtmlEncode(settings.Name ?? string.Empty);
            var safeColor = IsSafeColor(settings.Color) ? settings.Color : "inherit";

            return $"<span style=\"color:{safeColor}\">{safeName}</span>";
        }

        private static bool IsSafeColor(string color)
        {
            return !string.IsNullOrEmpty(color) && SafeColorPattern.IsMatch(color);
        }
    }
}
