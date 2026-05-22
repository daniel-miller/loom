using Xunit;

namespace Loom.Tests
{
    public class OrganizationSettingsTests
    {
        [Fact]
        public void Constructor_assigns_all_properties()
        {
            var s = new OrganizationSettings("red", "Red Organization", "red");
            Assert.Equal("red", s.Slug);
            Assert.Equal("Red Organization", s.Name);
            Assert.Equal("red", s.Color);
        }

        [Fact]
        public void Properties_are_get_only()
        {
            var type = typeof(OrganizationSettings);
            foreach (var prop in type.GetProperties())
            {
                Assert.True(prop.CanRead, $"{prop.Name} should be readable");
                Assert.False(prop.CanWrite, $"{prop.Name} should not be writable");
            }
        }
    }
}
