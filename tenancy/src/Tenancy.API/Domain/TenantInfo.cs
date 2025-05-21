using Finbuckle.MultiTenant.Abstractions;
using Tenancy.API.Domain.Enums;

namespace Tenancy.API.Model;

public class Tenant : ITenantInfo
{
    public string Id { get; set; } = default!;
    public string Identifier { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string ConnectionString { get; set; } = default!;

    public TenantType TenantType { get; set; }
    public string Description { get; set; } = default!;
}
