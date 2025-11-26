using Microsoft.EntityFrameworkCore;
using Backend.Data;

namespace backend.tests.Helpers;

public static class DbContextHelper
{
    public static EdmDbContext GetInMemoryDbContext(string dbName = null)
    {
        var options = new DbContextOptionsBuilder<EdmDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new EdmDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static void CleanupDbContext(EdmDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
}
