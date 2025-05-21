using Finbuckle.MultiTenant;
using Tenancy.API.Model;
using Tenancy.API.Services;

namespace Tenancy.API.Middlewares;

public class ContextMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var contextService = context.RequestServices.GetService<IContextService>();
        if (contextService == null)
            return;

        contextService.CurTenant = context.GetMultiTenantContext<Tenant>()?.TenantInfo;

        await next(context);
    }
}
