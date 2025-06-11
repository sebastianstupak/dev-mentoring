using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using Microsoft.EntityFrameworkCore;
using Tenancy.API.Model;

namespace Tenancy.API;

public class TenantDbContext : EFCoreStoreDbContext<Tenant>
{
    private readonly IConfiguration _configuration;

    public TenantDbContext(
        IConfiguration configuration,
        DbContextOptions<TenantDbContext> options)
        : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // For development use docker-compose
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("TenantDb"));
    }
}