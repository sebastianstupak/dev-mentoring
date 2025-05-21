# Multi-Tenant API with ASP.NET Core

This project demonstrates how to build a multi-tenant application with hybrid and isolated database model.

## What is Multi-Tenancy?

Multi-tenancy is an architecture pattern where a single application instance serves multiple customers (tenants). Each tenant's data is isolated and invisible to other tenants, but they all share the same application code and infrastructure.

### Common Multi-Tenancy Strategies

1. **Database per Tenant** (Used in this project)

   - Each tenant has a separate database
   - Complete data isolation
   - Easy to scale and backup individual tenants

2. **Schema per Tenant**

   - Shared database with separate schemas
   - Good balance between isolation and resource sharing

3. **Shared Database with Tenant ID** (Used in this project)
   - All tenants share the same database
   - Data is filtered by tenant identifier
   - Most cost-effective but least isolated

## How It Works

### 1. Tenant Identification

The application identifies tenants using the `X-Tenant` header in HTTP requests:

```http
GET /api/items
X-Tenant: company-a
```

NOTE: Additional options how to determine the tenant would be through JWT claims, uri, domain, etc.

### 2. Database Resolution

When a request comes in:

1. The `X-Tenant` header is extracted
2. Finbuckle.MultiTenant resolves the tenant configuration and we feed that into our ContextService iva middleware which can be further customized for other business logic
3. The appropriate database connection string is selected
4. All database operations use the tenant's specific database

### 3. Data Isolation

Each tenant's data is isolated:

- **Tenant A** → Database: `Shared_DB` (Isolated via query filter on TenantID property)
- **Tenant B** → Database: `Shared_DB` (Isolated via query filter on TenantID property)
- **Tenant C** → Database: `Isolated_DB`
