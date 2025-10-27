using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SavePoint.DAL.Contexts;
using SavePoint.Entities.Users;
using SavePoint.Entities.Games;
using SavePoint.Entities.Popularity;
using Bogus;
using System.Runtime.InteropServices;
using Hangfire;
using Hangfire.InMemory;
using Microsoft.Data.Sqlite;

namespace SavePoint.IntegrationTests.Fixtures
{
    public class WebApplicationFixture : WebApplicationFactory<Program>
    {
        private readonly bool _useInMemoryDatabase;
        private readonly string? _connectionString;
        private SqliteConnection? _sqliteConnection;

        public WebApplicationFixture()
        {
            // Use SQLite in-memory on Linux (CI), LocalDB on Windows (local dev)
            _useInMemoryDatabase = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            
            if (_useInMemoryDatabase)
            {
                // Create a persistent in-memory SQLite connection
                // This connection must stay open for the lifetime of the tests
                _sqliteConnection = new SqliteConnection("DataSource=:memory:");
                _sqliteConnection.Open();
            }
            else
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

                // Remove Hangfire server (background job processor) to prevent hanging
                // Remove ALL Hangfire-related hosted services
                var hangfireHostedServices = services
                    .Where(d => d.ServiceType == typeof(IHostedService) && 
                               (d.ImplementationType?.Namespace?.StartsWith("Hangfire") == true ||
                                d.ImplementationType?.Name == "JobInitializationService"))
                    .ToList();
                
                foreach (var service in hangfireHostedServices)
                {
                    services.Remove(service);
                }

                // Re-add Hangfire with InMemory storage for tests (without server)
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
                        // Use the persistent SQLite connection
                        options.UseSqlite(_sqliteConnection!);
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

                // Add popularity scores for different popularity types
                for (int popularityType = 1; popularityType <= 3; popularityType++)
                {
                    var popularity = new Popularity
                    {
                        Id = Guid.NewGuid(),
                        GameId = game.Id,
                        PopularityType = popularityType,
                        PopularityScore = Random.Shared.Next(50, 100),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    context.Popularities.Add(popularity);
                }
            }

            context.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (_useInMemoryDatabase)
                    {
                        // Close and dispose the SQLite connection
                        _sqliteConnection?.Close();
                        _sqliteConnection?.Dispose();
                    }
                    else
                    {
                        // Clean up the LocalDB database
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