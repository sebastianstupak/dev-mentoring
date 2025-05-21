namespace Tenancy.API.Model;

public class Item
{
    public int Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; } = default!;

    public string TenantId { get; set; } = default!;
}
