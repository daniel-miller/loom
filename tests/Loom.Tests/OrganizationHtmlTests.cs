using Xunit;

namespace Loom.Tests
{
    public class OrganizationHtmlTests
    {
        [Fact]
        public void ColoredName_html_encodes_name()
        {
            var settings = new OrganizationSettings(
                slug: "x",
                name: "<script>alert(1)</script>",
                color: "red");

            var html = OrganizationHtml.ColoredName(settings);

            Assert.DoesNotContain("<script>", html);
            Assert.Contains("&lt;script&gt;", html);
        }

        [Theory]
        [InlineData("red")]
        [InlineData("BLUE")]
        [InlineData("#fff")]
        [InlineData("#abcdef")]
        [InlineData("#aabbccdd")]
        public void ColoredName_accepts_safe_color(string color)
        {
            var settings = new OrganizationSettings("x", "Name", color);
            var html = OrganizationHtml.ColoredName(settings);
            Assert.Contains($"color:{color}", html);
        }

        [Theory]
        [InlineData("javascript:alert(1)")]
        [InlineData("url(foo)")]
        [InlineData("red; background:url(x)")]
        [InlineData("expression(alert(1))")]
        public void ColoredName_falls_back_when_color_is_unsafe(string color)
        {
            var settings = new OrganizationSettings("x", "Name", color);
            var html = OrganizationHtml.ColoredName(settings);
            Assert.Contains("color:inherit", html);
            Assert.DoesNotContain(color, html);
        }

        [Fact]
        public void ColoredName_handles_null_name()
        {
            var settings = new OrganizationSettings("x", null, "red");
            var html = OrganizationHtml.ColoredName(settings);
            Assert.Equal("<span style=\"color:red\"></span>", html);
        }
    }
}
