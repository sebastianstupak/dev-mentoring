using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Tenancy.API;
using Tenancy.API.Model;
using Tenancy.API.Services;

namespace Tenancy.API
{
    public class AppDbContext : DbContext, IMultiTenantDbContext
    {
        private readonly IContextService _contextService;

        public ITenantInfo? TenantInfo { get; set; }
        public TenantMismatchMode TenantMismatchMode => TenantMismatchMode.Throw;
        public TenantNotSetMode TenantNotSetMode => TenantNotSetMode.Throw;

        public DbSet<Item> Items { get; set; } = default!;

        public AppDbContext(
            IContextService contextService,
            DbContextOptions options)
            : base(options)
        {
            _contextService = contextService;

            if (TenantInfo == null &&
                _contextService != null &&
                _contextService.CurTenant != null)
            {
                TenantInfo = _contextService.CurTenant;
            }

            if (TenantInfo == null)
                throw new ArgumentNullException(nameof(TenantInfo));

            if (_contextService == null)
                throw new ArgumentNullException(nameof(ContextService));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var tenant = TenantInfo as Tenant ?? throw new NullReferenceException("Tenant is not set");
            optionsBuilder.UseNpgsql(tenant.ConnectionString);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>()
                .IsMultiTenant();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.EnforceMultiTenant();

            // NOTE: Enforce other stuff if needed

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        return new AppDbContext(
            new ContextService() { CurTenant = new Tenant { ConnectionString = "dummy" } },
            new DbContextOptions<AppDbContext>());
    }
}
