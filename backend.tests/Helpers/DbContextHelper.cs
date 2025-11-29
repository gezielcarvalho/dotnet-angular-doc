using Microsoft.EntityFrameworkCore;
using Backend.Data;

namespace backend.tests.Helpers;

public static class DbContextHelper
{
    public static DocumentDbContext GetInMemoryDbContext(string dbName = null)
    {
        var options = new DbContextOptionsBuilder<DocumentDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new DocumentDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static void CleanupDbContext(DocumentDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
}
