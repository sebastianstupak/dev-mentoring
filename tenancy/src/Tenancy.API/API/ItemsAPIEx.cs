using Microsoft.EntityFrameworkCore;
using Tenancy.API.Domain.Enums;
using Tenancy.API.Model;

namespace Tenancy.API.API;

public static class ItemsAPIEx
{
    private record CreateItemRequest
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; } = default!;
    }

    public static WebApplication MapItemEndpoints(this WebApplication app)
    {
        app.MapGet("/api/items",
            async (AppDbContext dbContext) =>
            {
                return await dbContext.Items.ToListAsync();
            })
            .WithName("GetItems");

        app.MapGet("/api/items-bypass",
            async (AppDbContext dbContext) =>
            {
                return await dbContext.Items.IgnoreQueryFilters().ToListAsync();
            })
            .WithName("GetItemsBypass");

        app.MapGet("/api/items/{id}",
            async (int id, AppDbContext dbContext) =>
            {
                var item = await dbContext.Items.FindAsync(id);
                return item != null ? Results.Ok(item) : Results.NotFound();
            })
            .WithName("GetItem");

        app.MapPost("/api/items",
            async (CreateItemRequest req, AppDbContext dbContext) =>
            {
                var item = new Item
                {
                    Name = req.Name,
                    Description = req.Description,
                    Price = req.Price,
                };
                dbContext.Items.Add(item);
                await dbContext.SaveChangesAsync();
                return Results.Created($"/api/items/{item.Id}", item);
            })
            .WithName("CreateItem");

        app.MapPut("/api/items/{id}",
            async (int id, Item updatedItem, AppDbContext dbContext) =>
            {
                var item = await dbContext.Items.FindAsync(id);
                if (item == null)
                    return Results.NotFound();

                item.Name = updatedItem.Name;

                await dbContext.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("UpdateItem");

        app.MapDelete("/api/items/{id}",
            async (int id, AppDbContext dbContext) =>
            {
                var item = await dbContext.Items.FindAsync(id);
                if (item == null)
                    return Results.NotFound();

                dbContext.Items.Remove(item);
                await dbContext.SaveChangesAsync();
                return Results.NoContent();
            })
            .WithName("DeleteItem");

        return app;
    }
}
