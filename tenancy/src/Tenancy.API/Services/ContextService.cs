using Tenancy.API.Model;

namespace Tenancy.API.Services;

public interface IContextService
{
    Tenant? CurTenant { get; set; }
}

public class ContextService : IContextService
{
    public Tenant? CurTenant { get; set; }
}