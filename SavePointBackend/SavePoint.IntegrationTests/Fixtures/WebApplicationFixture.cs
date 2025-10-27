using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SavePoint.DAL.Contexts;
using SavePoint.Entities.Users;
using SavePoint.Entities.Games;
using Bogus;
using System.Runtime.InteropServices;
using Hangfire;
using Hangfire.InMemory;

namespace SavePoint.IntegrationTests.Fixtures
{
    public class WebApplicationFixture : WebApplicationFactory<Program>
    {
        private readonly bool _useInMemoryDatabase;
        private readonly string? _connectionString;

        public WebApplicationFixture()
        {
            // Use SQLite in-memory on Linux (CI), LocalDB on Windows (local dev)
            _useInMemoryDatabase = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            
            if (!_useInMemoryDatabase)
            {
                _connectionString = $"Server=(localdb)\\mssqllocaldb;Database=SavePointIntegrationTest_{Guid.NewGuid()};Trusted_Connection=true;MultipleActiveResultSets=true";
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                if (!_useInMemoryDatabase && _connectionString != null)
                {
                    // Load test configuration for LocalDB
                    config.AddJsonFile("appsettings.Test.json", optional: false)
                          .AddInMemoryCollection(new Dictionary<string, string?>
                          {
                              ["ConnectionStrings:DefaultConnection"] = _connectionString
                          });
                }
                else
                {
                    // Load test configuration for SQLite
                    config.AddJsonFile("appsettings.Test.json", optional: false);
                }
            });

            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Replace Hangfire SQL Server storage with InMemory storage
                // Find and remove only the IGlobalConfiguration service (Hangfire configuration)
                var hangfireConfig = services.FirstOrDefault(d => d.ServiceType == typeof(IGlobalConfiguration));
                if (hangfireConfig != null)
                {
                    services.Remove(hangfireConfig);
                }

                // Re-add Hangfire with InMemory storage for tests
                services.AddHangfire(config =>
                {
                    config
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseInMemoryStorage();
                });

                // Add test database - SQLite in-memory on Linux, LocalDB on Windows
                if (_useInMemoryDatabase)
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlite($"DataSource=InMemoryTestDb_{Guid.NewGuid()};Mode=Memory;Cache=Shared");
                    });
                }
                else
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlServer(_connectionString!);
                    });
                }

                // Ensure database is created and seeded
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
                SeedTestData(context);
            });

            builder.UseEnvironment("Test");
        }

        private static void SeedTestData(ApplicationDbContext context)
        {
            if (context.Games.Any()) return; // Already seeded

            // Create test genres
            var genres = new List<Genre>
            {
                new Genre { Id = Guid.NewGuid(), Name = "Action", ExternalId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Genre { Id = Guid.NewGuid(), Name = "Adventure", ExternalId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Genre { Id = Guid.NewGuid(), Name = "RPG", ExternalId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            context.Genres.AddRange(genres);

            // Create test platforms
            var platforms = new List<Platform>
            {
                new Platform { Id = Guid.NewGuid(), Name = "PC", Abbreviation = "PC", ExternalId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Platform { Id = Guid.NewGuid(), Name = "PlayStation 5", Abbreviation = "PS5", ExternalId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Platform { Id = Guid.NewGuid(), Name = "Xbox Series X", Abbreviation = "XSX", ExternalId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            context.Platforms.AddRange(platforms);

            // Create test companies
            var companies = new List<Company>
            {
                new Company { Id = Guid.NewGuid(), Name = "Test Studios", ExternalId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Company { Id = Guid.NewGuid(), Name = "Game Publishers Inc", ExternalId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            context.Companies.AddRange(companies);

            // Create test games using Bogus
            var faker = new Faker<Game>()
                .RuleFor(g => g.Id, f => Guid.NewGuid())
                .RuleFor(g => g.Name, f => f.Commerce.ProductName())
                .RuleFor(g => g.Summary, f => f.Lorem.Paragraph())
                .RuleFor(g => g.Rating, f => f.Random.Double(1, 10))
                .RuleFor(g => g.ReleaseDate, f => f.Date.Past(10))
                .RuleFor(g => g.CoverUrl, f => f.Image.PicsumUrl())
                .RuleFor(g => g.ExternalId, f => f.Random.Long(1000, 9999))
                .RuleFor(g => g.CreatedAt, f => DateTime.UtcNow)
                .RuleFor(g => g.UpdatedAt, f => DateTime.UtcNow);

            var games = faker.Generate(10);
            context.Games.AddRange(games);

            context.SaveChanges();

            // Create game relationships
            foreach (var game in games.Take(5))
            {
                // Add some genres to games
                var gameGenre = new GameGenre
                {
                    Id = Guid.NewGuid(),
                    GameId = game.Id,
                    GenreId = genres[Random.Shared.Next(genres.Count)].Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.GameGenres.Add(gameGenre);

                // Add some platforms to games
                var gamePlatform = new GamePlatform
                {
                    Id = Guid.NewGuid(),
                    GameId = game.Id,
                    PlatformId = platforms[Random.Shared.Next(platforms.Count)].Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.GamePlatforms.Add(gamePlatform);
            }

            context.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    // Clean up the database (only needed for LocalDB, SQLite in-memory auto-cleans)
                    if (!_useInMemoryDatabase)
                    {
                        using var scope = Services.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        context.Database.EnsureDeleted();
                    }
                }
                catch
                {
                    // Ignore cleanup errors during disposal
                }
            }
            base.Dispose(disposing);
        }
    }
}