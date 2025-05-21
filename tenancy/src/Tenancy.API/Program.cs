using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Tenancy.API;
using Tenancy.API.API;
using Tenancy.API.Middlewares;
using Tenancy.API.Model;
using Tenancy.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMultiTenant<Tenant>()
    .WithHeaderStrategy("X-Tenant")
    .WithEFCoreStore<TenantDbContext, Tenant>();

builder.Services.AddDbContext<AppDbContext>();

builder.Services.AddScoped<IContextService, ContextService>();
builder.Services.AddScoped<ContextMiddleware>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tenancy API", Version = "v1" });
    c.AddSecurityDefinition("Tenant header", new OpenApiSecurityScheme
    {
        Name = "X-Tenant",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Tenant identifier"
    });
    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Tenant header"
                }
            },
            Array.Empty<string>()
        }
    };
    c.AddSecurityRequirement(securityRequirement);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var tenantDbCtx = services.GetRequiredService<TenantDbContext>();
    await tenantDbCtx.Database.MigrateAsync();

    // NOTE: Very simple migration handling for all tenants
    var dbTenants = await tenantDbCtx.TenantInfo.ToListAsync();
    foreach (var dbTenant in dbTenants)
    {
        var appDbCtx = new AppDbContext(
            new ContextService() { CurTenant = dbTenant },
            new DbContextOptions<AppDbContext>());
        await appDbCtx.Database.MigrateAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMultiTenant();
app.UseMiddleware<ContextMiddleware>();

app.MapTenantEndpoints();
app.MapItemEndpoints();

app.Run();