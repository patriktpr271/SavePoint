using AutoMapper;
using SavePoint.BusinessLogic.Services;
using SavePoint.Common.Dtos.Games;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Common;
using SavePoint.Entities.Games;
using SavePoint.Entities.Popularity;

namespace SavePoint.BusinessLogic.Tests.Services
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GameService _sut;

        public GameServiceTests()
        {
            _gameRepositoryMock = new Mock<IGameRepository>();
            _mapperMock = new Mock<IMapper>();
            _sut = new GameService(_gameRepositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetGames_WithMatchingResults_ReturnsPagedResultWithMappedItems()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game { Id = Guid.NewGuid(), Name = "Halo" },
                new Game { Id = Guid.NewGuid(), Name = "Doom" }
            };
            var pagedGames = new PagedResult<Game>
            {
                Items = games,
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 20
            };
            var mappedDtos = new List<GameCardDto>
            {
                new GameCardDto { Id = games[0].Id, Name = "Halo" },
                new GameCardDto { Id = games[1].Id, Name = "Doom" }
            };
            _gameRepositoryMock
                .Setup(r => r.GetGames(1, 20, null, null, "asc", null, null, null, null, null, null, null))
                .ReturnsAsync(pagedGames);
            _mapperMock.Setup(m => m.Map<List<GameCardDto>>(games)).Returns(mappedDtos);

            // Act
            var result = await _sut.GetGames();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(20);
            result.Items.Should().AllSatisfy(i => i.PopularityScore.Should().BeNull());
        }

        [Fact]
        public async Task GetGames_WithEmptyRepositoryResult_ReturnsEmptyPagedResult()
        {
            // Arrange
            var pagedGames = new PagedResult<Game>
            {
                Items = new List<Game>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 20
            };
            _gameRepositoryMock
                .Setup(r => r.GetGames(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<double?>(), It.IsAny<double?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<Guid[]?>(), It.IsAny<Guid[]?>(), It.IsAny<Guid[]?>()))
                .ReturnsAsync(pagedGames);
            _mapperMock
                .Setup(m => m.Map<List<GameCardDto>>(It.IsAny<IList<Game>>()))
                .Returns(new List<GameCardDto>());

            // Act
            var result = await _sut.GetGames();

            // Assert
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetGames_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _gameRepositoryMock
                .Setup(r => r.GetGames(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<double?>(), It.IsAny<double?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<Guid[]?>(), It.IsAny<Guid[]?>(), It.IsAny<Guid[]?>()))
                .ThrowsAsync(new InvalidOperationException("DB unreachable"));

            // Act
            Func<Task> act = async () => await _sut.GetGames();

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("DB unreachable");
        }

        [Fact]
        public async Task GetPopularGames_WithMatchingPopularityType_PopulatesPopularityScore()
        {
            // Arrange
            const int popularityType = 1;
            var gameId = Guid.NewGuid();
            var game = new Game
            {
                Id = gameId,
                Name = "Witcher 3",
                Popularities = new List<Popularity>
                {
                    new Popularity { PopularityType = popularityType, PopularityScore = 95.5m },
                    new Popularity { PopularityType = 2, PopularityScore = 80m }
                }
            };
            var pagedGames = new PagedResult<Game>
            {
                Items = new List<Game> { game },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 20
            };
            var dto = new GameCardDto { Id = gameId, Name = "Witcher 3" };
            _gameRepositoryMock
                .Setup(r => r.GetPopularGames(popularityType, 1, 20))
                .ReturnsAsync(pagedGames);
            _mapperMock
                .Setup(m => m.Map<List<GameCardDto>>(pagedGames.Items))
                .Returns(new List<GameCardDto> { dto });

            // Act
            var result = await _sut.GetPopularGames(popularityType);

            // Assert
            result.Items.Should().ContainSingle();
            result.Items.First().PopularityScore.Should().Be(95.5m);
        }

        [Fact]
        public async Task GetPopularGames_WithNoMatchingPopularityType_LeavesPopularityScoreNull()
        {
            // Arrange
            const int requestedType = 99;
            var gameId = Guid.NewGuid();
            var game = new Game
            {
                Id = gameId,
                Name = "Stardew",
                Popularities = new List<Popularity>
                {
                    new Popularity { PopularityType = 1, PopularityScore = 50m }
                }
            };
            var pagedGames = new PagedResult<Game>
            {
                Items = new List<Game> { game },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 20
            };
            var dto = new GameCardDto { Id = gameId, Name = "Stardew" };
            _gameRepositoryMock
                .Setup(r => r.GetPopularGames(requestedType, 1, 20))
                .ReturnsAsync(pagedGames);
            _mapperMock
                .Setup(m => m.Map<List<GameCardDto>>(pagedGames.Items))
                .Returns(new List<GameCardDto> { dto });

            // Act
            var result = await _sut.GetPopularGames(requestedType);

            // Assert
            result.Items.First().PopularityScore.Should().BeNull();
        }

        [Fact]
        public async Task GetPopularGames_WhenRepositoryReturnsItemMissingFromMappedList_ThrowsInvalidOperation()
        {
            // Arrange — mismatch between mapped DTOs and source items breaks the .First() lookup
            var sourceGame = new Game { Id = Guid.NewGuid(), Name = "A", Popularities = new List<Popularity>() };
            var pagedGames = new PagedResult<Game>
            {
                Items = new List<Game> { sourceGame },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 20
            };
            var dtoWithDifferentId = new GameCardDto { Id = Guid.NewGuid(), Name = "A" };
            _gameRepositoryMock
                .Setup(r => r.GetPopularGames(1, 1, 20))
                .ReturnsAsync(pagedGames);
            _mapperMock
                .Setup(m => m.Map<List<GameCardDto>>(pagedGames.Items))
                .Returns(new List<GameCardDto> { dtoWithDifferentId });

            // Act
            Func<Task> act = async () => await _sut.GetPopularGames(1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task GetGameById_WithExistingGame_ReturnsDtoWithHighestPopularityScore()
        {
            // Arrange
            var id = Guid.NewGuid();
            var game = new Game
            {
                Id = id,
                Name = "Elden Ring",
                Popularities = new List<Popularity>
                {
                    new Popularity { PopularityScore = 70m },
                    new Popularity { PopularityScore = 99m },
                    new Popularity { PopularityScore = 50m }
                }
            };
            var dto = new GameDetailDto { Id = id, Name = "Elden Ring" };
            _gameRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(id)).ReturnsAsync(game);
            _mapperMock.Setup(m => m.Map<GameDetailDto>(game)).Returns(dto);

            // Act
            var result = await _sut.GetGameById(id);

            // Assert
            result.Should().NotBeNull();
            result!.PopularityScore.Should().Be(99m);
        }

        [Fact]
        public async Task GetGameById_WithUnknownId_ReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _gameRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(id)).ReturnsAsync((Game?)null);

            // Act
            var result = await _sut.GetGameById(id);

            // Assert
            result.Should().BeNull();
            _mapperMock.Verify(m => m.Map<GameDetailDto>(It.IsAny<Game>()), Times.Never);
        }

        [Fact]
        public async Task GetGameById_WithGameHavingNoPopularities_LeavesPopularityScoreNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            var game = new Game { Id = id, Name = "Indie", Popularities = new List<Popularity>() };
            var dto = new GameDetailDto { Id = id, Name = "Indie" };
            _gameRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(id)).ReturnsAsync(game);
            _mapperMock.Setup(m => m.Map<GameDetailDto>(game)).Returns(dto);

            // Act
            var result = await _sut.GetGameById(id);

            // Assert
            result.Should().NotBeNull();
            result!.PopularityScore.Should().BeNull();
        }
    }
}
