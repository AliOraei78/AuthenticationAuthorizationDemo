using AuthenticationAuthorizationDemo.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace AuthenticationAuthorizationDemo.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1. Remove the application's DbContext registration
                // Note: Replace 'ApplicationDbContext' with your actual DbContext class name
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 2. Create and open a persistent SQLite connection
                // In-memory SQLite databases are destroyed when the connection closes.
                // Keeping it open here ensures the database lives for the entire test run.
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                // 3. Add the DbContext using the persistent test connection
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                // 4. Ensure the database is created BEFORE the seeder runs
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                    // This creates the AspNetRoles table and other Identity tables
                    db.Database.EnsureCreated();
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // Clean up the connection when the factory is disposed
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}