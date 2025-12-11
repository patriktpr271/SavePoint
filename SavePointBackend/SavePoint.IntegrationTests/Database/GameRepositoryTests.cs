using SavePoint.IntegrationTests.Fixtures;
using SavePoint.DAL.Repositories;
using SavePoint.Entities.Games;
using Bogus;

namespace SavePoint.IntegrationTests.Database
{
    public class GameRepositoryTests : IClassFixture<WebApplicationFixture>
    {
        private readonly WebApplicationFixture _factory;

        public GameRepositoryTests(WebApplicationFixture factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetGames_WithNoFilters_ReturnsAllGames()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repository = new GameRepository(context);

            // Act
            var result = await repository.GetGames();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
            result.TotalCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetGames_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repository = new GameRepository(context);

            var pageNumber = 1;
            var pageSize = 3;

            // Act
            var result = await repository.GetGames(pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.PageNumber.Should().Be(pageNumber);
            result.PageSize.Should().Be(pageSize);
            result.Items.Should().HaveCountLessOrEqualTo(pageSize);
        }

        [Fact]
        public async Task GetGames_WithSearchFilter_ReturnsMatchingGames()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repository = new GameRepository(context);

            // First, add a game with a specific name for testing
            var testGame = new Game
            {
                Id = Guid.NewGuid(),
                Name = "Unique Test Game For Search",
                Summary = "Test summary",
                Rating = 8.5,
                ReleaseDate = DateTime.Now.AddYears(-1),
                ExternalId = 99999,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            context.Games.Add(testGame);
            await context.SaveChangesAsync();

            var searchTerm = "Unique Test Game";

            // Act
            var result = await repository.GetGames(search: searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
            result.Items.Should().Contain(g => g.Name.Contains(searchTerm));
        }

        [Fact]
        public async Task GetGames_WithSorting_ReturnsSortedResults()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repository = new GameRepository(context);

            // Act
            var resultAsc = await repository.GetGames(sortBy: "name", sortOrder: "asc");
            var resultDesc = await repository.GetGames(sortBy: "name", sortOrder: "desc");

            // Assert
            resultAsc.Should().NotBeNull();
            resultDesc.Should().NotBeNull();

            if (resultAsc.Items.Count > 1)
            {
                var namesAsc = resultAsc.Items.Select(g => g.Name).ToList();
                namesAsc.Should().BeInAscendingOrder();
            }

            if (resultDesc.Items.Count > 1)
            {
                var namesDesc = resultDesc.Items.Select(g => g.Name).ToList();
                namesDesc.Should().BeInDescendingOrder();
            }
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_WithValidId_ReturnsGameWithRelatedData()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repository = new GameRepository(context);

            // Get a game ID from the database
            var existingGame = await context.Games.FirstAsync();

            // Act
            var result = await repository.GetByIdWithDetailsAsync(existingGame.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(existingGame.Id);
            result.Name.Should().NotBeNullOrEmpty();
            
            // Navigation properties should be loaded
            result.GameGenres.Should().NotBeNull();
            result.GamePlatforms.Should().NotBeNull();
            result.GameCompanies.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repository = new GameRepository(context);

            var invalidId = Guid.NewGuid();

            // Act
            var result = await repository.GetByIdWithDetailsAsync(invalidId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task InsertOrUpdateAsync_WithNewGame_InsertsGame()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repository = new GameRepository(context);

            var newGame = new Faker<Game>()
                .RuleFor(g => g.Id, f => Guid.NewGuid())
                .RuleFor(g => g.Name, f => f.Commerce.ProductName())
                .RuleFor(g => g.Summary, f => f.Lorem.Paragraph())
                .RuleFor(g => g.Rating, f => f.Random.Double(1, 10))
                .RuleFor(g => g.ReleaseDate, f => f.Date.Past(5))
                .RuleFor(g => g.ExternalId, f => f.Random.Long(10000, 99999))
                .RuleFor(g => g.CreatedAt, f => DateTime.UtcNow)
                .RuleFor(g => g.UpdatedAt, f => DateTime.UtcNow)
                .Generate();

            // Act
            await repository.InsertOrUpdateAsync(newGame);

            // Assert
            var insertedGame = await context.Games.FindAsync(newGame.Id);
            insertedGame.Should().NotBeNull();
            insertedGame!.Name.Should().Be(newGame.Name);
        }

        [Fact]
        public async Task InsertOrUpdateAsync_WithExistingGame_UpdatesGame()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repository = new GameRepository(context);

            // Get an existing game
            var existingGame = await context.Games.FirstAsync();
            var originalName = existingGame.Name;
            var updatedName = "Updated Game Name " + Guid.NewGuid();

            // Modify the game
            existingGame.Name = updatedName;
            existingGame.UpdatedAt = DateTime.UtcNow;

            // Act
            await repository.InsertOrUpdateAsync(existingGame);

            // Assert
            var updatedGame = await context.Games.FindAsync(existingGame.Id);
            updatedGame.Should().NotBeNull();
            updatedGame!.Name.Should().Be(updatedName);
            updatedGame.Name.Should().NotBe(originalName);
        }

        [Fact]
        public async Task GetExistingExternalIdsAsync_WithValidIds_ReturnsMatchingIds()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repository = new GameRepository(context);

            // Get some existing external IDs
            var existingGames = await context.Games.Take(3).ToListAsync();
            var existingExternalIds = existingGames
                .Where(g => g.ExternalId.HasValue)
                .Select(g => g.ExternalId!.Value)
                .ToList();

            // Add some non-existing IDs
            var testIds = existingExternalIds.Concat(new[] { 999999L, 999998L }).ToList();

            // Act
            var result = await repository.GetExistingExternalIdsAsync(testIds);

            // Assert
            result.Should().NotBeNull();
            result.Should().OnlyContain(id => existingExternalIds.Contains(id));
            result.Should().NotContain(999999L);
            result.Should().NotContain(999998L);
        }

        [Fact]
        public async Task GetGames_WithRatingFilter_ReturnsFilteredResults()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repository = new GameRepository(context);

            var minRating = 5.0;
            var maxRating = 9.0;

            // Act
            var result = await repository.GetGames(minRating: minRating, maxRating: maxRating);

            // Assert
            result.Should().NotBeNull();
            
            foreach (var game in result.Items)
            {
                game.Rating.Should().BeInRange(minRating, maxRating);
            }
        }
    }
}