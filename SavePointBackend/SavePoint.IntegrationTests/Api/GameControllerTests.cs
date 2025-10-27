using SavePoint.IntegrationTests.Fixtures;
using System.Net;

namespace SavePoint.IntegrationTests.Api
{
    public class GameControllerTests : IClassFixture<WebApplicationFixture>
    {
        private readonly WebApplicationFixture _factory;
        private readonly HttpClient _client;

        public GameControllerTests(WebApplicationFixture factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetGames_ReturnsPagedResults()
        {
            // Act
            var response = await _client.GetAsync("/api/game");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
            
            // Should contain pagination structure
            content.Should().Contain("totalCount");
            content.Should().Contain("pageNumber");
            content.Should().Contain("pageSize");
            content.Should().Contain("items");
        }

        [Fact]
        public async Task GetGames_WithPagination_ReturnsCorrectPageSize()
        {
            // Arrange
            var pageSize = 5;
            
            // Act
            var response = await _client.GetAsync($"/api/game?pageSize={pageSize}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var pagedResult = await response.Content.ReadFromJsonAsync<PagedGameResult>();
            pagedResult.Should().NotBeNull();
            pagedResult!.PageSize.Should().Be(pageSize);
            pagedResult.Items.Should().HaveCountLessOrEqualTo(pageSize);
        }

        [Fact]
        public async Task GetGames_WithSearchFilter_ReturnsFilteredResults()
        {
            // Arrange
            var searchTerm = "Game"; // This should match some test data
            
            // Act
            var response = await _client.GetAsync($"/api/game?search={searchTerm}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var pagedResult = await response.Content.ReadFromJsonAsync<PagedGameResult>();
            pagedResult.Should().NotBeNull();
            
            // All returned games should contain the search term (case-insensitive)
            foreach (var game in pagedResult!.Items)
            {
                game.Name.Should().ContainEquivalentOf(searchTerm);
            }
        }

        [Fact]
        public async Task GetGames_WithSorting_ReturnsSortedResults()
        {
            // Act
            var response = await _client.GetAsync("/api/game?sortBy=name&sortOrder=asc");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var pagedResult = await response.Content.ReadFromJsonAsync<PagedGameResult>();
            pagedResult.Should().NotBeNull();
            
            if (pagedResult!.Items.Count > 1)
            {
                var names = pagedResult.Items.Select(g => g.Name).ToList();
                names.Should().BeInAscendingOrder();
            }
        }

        [Fact]
        public async Task GetGameById_WithValidId_ReturnsGameDetails()
        {
            // Arrange - First get a game ID from the list
            var gamesResponse = await _client.GetAsync("/api/game");
            var pagedResult = await gamesResponse.Content.ReadFromJsonAsync<PagedGameResult>();
            
            if (pagedResult!.Items.Count == 0)
            {
                return; // Skip test if no games available
            }
            
            var gameId = pagedResult.Items.First().Id;

            // Act
            var response = await _client.GetAsync($"/api/game/{gameId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var game = await response.Content.ReadFromJsonAsync<GameDetailDto>();
            game.Should().NotBeNull();
            game!.Id.Should().Be(gameId);
            game.Name.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetGameById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/game/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPopularGames_ReturnsGamesWithPopularityScores()
        {
            // Arrange
            var popularityType = 1;

            // Act
            var response = await _client.GetAsync($"/api/game/popular/{popularityType}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var pagedResult = await response.Content.ReadFromJsonAsync<PagedGameResult>();
            pagedResult.Should().NotBeNull();
            
            // Popular games endpoint should return games
            pagedResult!.Items.Should().NotBeNull();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(101)]
        public async Task GetGames_WithInvalidPageSize_HandlesProperly(int invalidPageSize)
        {
            // Act
            var response = await _client.GetAsync($"/api/game?pageSize={invalidPageSize}");

            // Assert
            // Should either return BadRequest or handle gracefully with default values
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetGames_WithYearFilter_ReturnsFilteredResults()
        {
            // Arrange
            var fromYear = 2020;
            var toYear = 2024;

            // Act
            var response = await _client.GetAsync($"/api/game?fromYear={fromYear}&toYear={toYear}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var pagedResult = await response.Content.ReadFromJsonAsync<PagedGameResult>();
            pagedResult.Should().NotBeNull();
            
            // All returned games should be within the year range
            foreach (var game in pagedResult!.Items)
            {
                game.ReleaseDate.Year.Should().BeInRange(fromYear, toYear);
            }
        }

        // Helper class for deserializing API responses
        private class PagedGameResult
        {
            public List<GameCardDto> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }
    }
}