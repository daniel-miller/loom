namespace Loom
{
    public interface IOrganizationContext
    {
        string Slug { get; }

        OrganizationSettings Settings { get; }
    }
}