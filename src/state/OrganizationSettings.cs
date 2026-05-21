namespace Loom
{
    public sealed class OrganizationSettings
    {
        public OrganizationSettings(string slug, string name, string color)
        {
            Slug = slug;
            Name = name;
            Color = color;
        }

        public string Color { get; }
        public string Name { get; }
        public string Slug { get; }
    }
}
