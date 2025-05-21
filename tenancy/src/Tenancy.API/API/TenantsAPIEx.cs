using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using Tenancy.API.Domain.Enums;
using Tenancy.API.Model;
using Tenancy.API.Services;

namespace Tenancy.API.API;

public static class TenantsAPIEx
{
    private class TenantResponse
    {
        public string Id { get; set; } = default!;
        public string Identifier { get; set; } = default!;
        public string Name { get; set; } = default!;
        public TenantType TenantType { get; set; }
        public string Description { get; set; } = default!;

        public static TenantResponse FromTenant(Tenant tenant)
        {
            return new TenantResponse
            {
                Id = tenant.Id,
                Identifier = tenant.Identifier,
                Name = tenant.Name,
                TenantType = tenant.TenantType,
                Description = tenant.Description,
            };
        }
    }

    private record CreateTenantRequest
    {
        public string Id { get; set; } = default!;
        public string Identifier { get; set; } = default!;
        public string Name { get; set; } = default!;
        public TenantType TenantType { get; set; }
        public string Description { get; set; } = default!;
    }

    private record UpdateTenantRequest
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
    }

    public static WebApplication MapTenantEndpoints(this WebApplication app)
    {
        app.MapGet("/api/tenants",
            async (TenantDbContext dbContext) =>
            {
                var dbTenants = await dbContext.TenantInfo.ToListAsync();
                return dbTenants.Select(x => TenantResponse.FromTenant(x));
            
            })
            .WithName("GetTenants");

        app.MapGet("/api/tenants/{id}",
            async (string id, TenantDbContext dbContext) =>
            {
                var tenant = await dbContext.TenantInfo.FindAsync(id);
                return tenant != null ? Results.Ok(TenantResponse.FromTenant(tenant)) : Results.NotFound();
            })
            .WithName("GetTenant");

        app.MapPost("/api/tenants",
            async (CreateTenantRequest req, TenantDbContext dbContext, IConfiguration configuration, IServiceProvider serviceProvider) =>
            {
                var connString = req.TenantType switch
                {
                    TenantType.Shared => configuration.GetConnectionString("SharedAppDb"),
                    TenantType.Isolated => configuration.GetConnectionString("IsolatedAppDb"),
                    _ => throw new ArgumentException($"Unsupported tenant type: {req.TenantType}")
                };

                if (string.IsNullOrEmpty(connString))
                    throw new Exception("Couldn't retrieve connection string");

                if (await dbContext.TenantInfo.AnyAsync(t => t.Identifier == req.Identifier))
                    return Results.Conflict($"Tenant with Identifier '{req.Identifier}' already exists");

                // Add tenant to database
                var tenant = new Tenant
                {
                    Id = req.Id,
                    Identifier = req.Identifier,
                    Name = req.Name,
                    ConnectionString = connString,
                    TenantType = req.TenantType,
                    Description = req.Description,

                };
                dbContext.TenantInfo.Add(tenant);
                await dbContext.SaveChangesAsync();

                // Create and migrate the tenant's database
                try
                {
                    var appDbCtx = new AppDbContext(new ContextService { CurTenant = tenant }, new DbContextOptions<AppDbContext>());
                    await appDbCtx.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    // If database creation fails, remove the tenant
                    dbContext.TenantInfo.Remove(tenant);
                    await dbContext.SaveChangesAsync();
                    return Results.Problem($"Failed to create tenant database: {ex.Message}", statusCode: 500);
                }

                return Results.Created($"/api/tenants/{tenant.Id}", TenantResponse.FromTenant(tenant));
            })
            .WithName("CreateTenant");

        app.MapPut("/api/tenants/{id}",
            async (string id, UpdateTenantRequest updatedTenant, TenantDbContext dbContext) =>
            {
                var tenant = await dbContext.TenantInfo.FindAsync(id);
                if (tenant == null)
                    return Results.NotFound();

                tenant.Name = updatedTenant.Name;
                tenant.Description = updatedTenant.Description;

                await dbContext.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("UpdateTenant");

        app.MapDelete("/api/tenants/{id}",
            async (string id, TenantDbContext dbContext) =>
            {
                var tenant = await dbContext.TenantInfo.FindAsync(id);
                if (tenant == null)
                    return Results.NotFound();

                dbContext.TenantInfo.Remove(tenant);
                await dbContext.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("DeleteTenant");

        return app;
    }
}
