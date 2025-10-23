using Moq;
using AutoMapper;
using FluentAssertions;
using SavePoint.BusinessLogic.Services;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Games;
using SavePoint.Entities.Common;
using SavePoint.Common.Dtos.Games;
using SavePoint.Entities.Popularity;

namespace SavePoint.Tests.Services
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _mockGameRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            _mockGameRepository = new Mock<IGameRepository>();
            _mockMapper = new Mock<IMapper>();
            _gameService = new GameService(_mockGameRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetGames_WithValidParameters_ReturnsPagedResult()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game { Id = Guid.NewGuid(), Name = "Test Game 1" },
                new Game { Id = Guid.NewGuid(), Name = "Test Game 2" }
            };

            var pagedResult = new PagedResult<Game>
            {
                Items = games,
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 20
            };

            var expectedDtos = new List<GameCardDto>
            {
                new GameCardDto { Id = games[0].Id, Name = "Test Game 1" },
                new GameCardDto { Id = games[1].Id, Name = "Test Game 2" }
            };

            _mockGameRepository.Setup(x => x.GetGames(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), 
                It.IsAny<string?>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<int?>(), 
                It.IsAny<int?>(), It.IsAny<Guid[]?>(), It.IsAny<Guid[]?>(), It.IsAny<Guid[]?>()))
                .ReturnsAsync(pagedResult);

            _mockMapper.Setup(x => x.Map<List<GameCardDto>>(games))
                .Returns(expectedDtos);

            // Act
            var result = await _gameService.GetGames();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(20);

            _mockGameRepository.Verify(x => x.GetGames(1, 20, null, null, "asc", null, null, null, null, null, null, null), Times.Once);
            _mockMapper.Verify(x => x.Map<List<GameCardDto>>(games), Times.Once);
        }

        [Fact]
        public async Task GetGames_WithCustomParameters_PassesParametersCorrectly()
        {
            // Arrange
            var pageNumber = 2;
            var pageSize = 10;
            var search = "test";
            var sortBy = "name";
            var sortOrder = "desc";
            var minRating = 5.0;
            var maxRating = 10.0;
            var fromYear = 2020;
            var toYear = 2023;
            var platformIds = new[] { Guid.NewGuid() };
            var companyIds = new[] { Guid.NewGuid() };
            var genreIds = new[] { Guid.NewGuid() };

            var pagedResult = new PagedResult<Game>
            {
                Items = new List<Game>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _mockGameRepository.Setup(x => x.GetGames(
                pageNumber, pageSize, search, sortBy, sortOrder, minRating, maxRating, 
                fromYear, toYear, platformIds, companyIds, genreIds))
                .ReturnsAsync(pagedResult);

            _mockMapper.Setup(x => x.Map<List<GameCardDto>>(It.IsAny<List<Game>>()))
                .Returns(new List<GameCardDto>());

            // Act
            await _gameService.GetGames(pageNumber, pageSize, search, sortBy, sortOrder, 
                minRating, maxRating, fromYear, toYear, platformIds, companyIds, genreIds);

            // Assert
            _mockGameRepository.Verify(x => x.GetGames(
                pageNumber, pageSize, search, sortBy, sortOrder, minRating, maxRating, 
                fromYear, toYear, platformIds, companyIds, genreIds), Times.Once);
        }

        [Fact]
        public async Task GetPopularGames_WithValidParameters_ReturnsPagedResultWithPopularityScores()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var popularityType = 1;
            var game = new Game 
            { 
                Id = gameId, 
                Name = "Popular Game",
                Popularities = new List<Popularity>
                {
                    new Popularity { PopularityType = popularityType, PopularityScore = 85.5m }
                }
            };

            var pagedResult = new PagedResult<Game>
            {
                Items = new List<Game> { game },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 20
            };

            var gameDto = new GameCardDto { Id = gameId, Name = "Popular Game" };

            _mockGameRepository.Setup(x => x.GetPopularGames(popularityType, 1, 20))
                .ReturnsAsync(pagedResult);

            _mockMapper.Setup(x => x.Map<List<GameCardDto>>(It.IsAny<List<Game>>()))
                .Returns(new List<GameCardDto> { gameDto });

            // Act
            var result = await _gameService.GetPopularGames(popularityType);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().PopularityScore.Should().Be(85.5m);

            _mockGameRepository.Verify(x => x.GetPopularGames(popularityType, 1, 20), Times.Once);
        }

        [Fact]
        public async Task GetGameById_WithValidId_ReturnsGameDetailDto()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Game 
            { 
                Id = gameId, 
                Name = "Test Game",
                Popularities = new List<Popularity>
                {
                    new Popularity { PopularityType = 1, PopularityScore = 90.0m },
                    new Popularity { PopularityType = 2, PopularityScore = 85.0m }
                }
            };

            var expectedDto = new GameDetailDto { Id = gameId, Name = "Test Game" };

            _mockGameRepository.Setup(x => x.GetByIdWithDetailsAsync(gameId))
                .ReturnsAsync(game);

            _mockMapper.Setup(x => x.Map<GameDetailDto>(game))
                .Returns(expectedDto);

            // Act
            var result = await _gameService.GetGameById(gameId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(gameId);
            result.Name.Should().Be("Test Game");
            result.PopularityScore.Should().Be(90.0m); // Should pick the highest score

            _mockGameRepository.Verify(x => x.GetByIdWithDetailsAsync(gameId), Times.Once);
            _mockMapper.Verify(x => x.Map<GameDetailDto>(game), Times.Once);
        }

        [Fact]
        public async Task GetGameById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var gameId = Guid.NewGuid();

            _mockGameRepository.Setup(x => x.GetByIdWithDetailsAsync(gameId))
                .ReturnsAsync((Game?)null);

            // Act
            var result = await _gameService.GetGameById(gameId);

            // Assert
            result.Should().BeNull();

            _mockGameRepository.Verify(x => x.GetByIdWithDetailsAsync(gameId), Times.Once);
            _mockMapper.Verify(x => x.Map<GameDetailDto>(It.IsAny<Game>()), Times.Never);
        }
    }
}