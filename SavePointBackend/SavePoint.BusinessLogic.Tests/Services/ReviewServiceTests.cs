using AutoMapper;
using SavePoint.BusinessLogic.Services;
using SavePoint.Common.Dtos.Reviews;
using SavePoint.Common.Exceptions;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Games;
using SavePoint.Entities.Reviews;

namespace SavePoint.BusinessLogic.Tests.Services
{
    public class ReviewServiceTests
    {
        private readonly Mock<IReviewRepository> _reviewRepoMock;
        private readonly Mock<IGameRepository> _gameRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ReviewService _sut;

        public ReviewServiceTests()
        {
            _reviewRepoMock = new Mock<IReviewRepository>();
            _gameRepoMock = new Mock<IGameRepository>();
            _mapperMock = new Mock<IMapper>();
            _sut = new ReviewService(_reviewRepoMock.Object, _gameRepoMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_WithReviewsInRepository_ReturnsMappedDtos()
        {
            // Arrange
            var reviews = new List<Review>
            {
                new Review { Id = Guid.NewGuid(), UserId = "u1", GameId = Guid.NewGuid(), Rating = 4 }
            };
            var dtos = new List<ReviewDto>
            {
                new ReviewDto { Id = reviews[0].Id, Rating = 4 }
            };
            _reviewRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(reviews);
            _mapperMock.Setup(m => m.Map<IEnumerable<ReviewDto>>(reviews)).Returns(dtos);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(dtos);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingReview_ReturnsMappedDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var review = new Review { Id = id, UserId = "u1", GameId = Guid.NewGuid(), Rating = 5 };
            var dto = new ReviewDto { Id = id, Rating = 5 };
            _reviewRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(review);
            _mapperMock.Setup(m => m.Map<ReviewDto>(review)).Returns(dto);

            // Act
            var result = await _sut.GetByIdAsync(id);

            // Assert
            result.Should().BeSameAs(dto);
        }

        [Fact]
        public async Task GetByIdAsync_WithMissingReview_ThrowsNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _reviewRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Review?)null);

            // Act
            Func<Task> act = async () => await _sut.GetByIdAsync(id);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .Where(e => e.ResourceType == "Review");
        }

        [Fact]
        public async Task GetUserReviewForGameAsync_WhenNoReview_ReturnsNull()
        {
            // Arrange
            _reviewRepoMock
                .Setup(r => r.GetUserReviewForGameAsync("u1", It.IsAny<Guid>()))
                .ReturnsAsync((Review?)null);

            // Act
            var result = await _sut.GetUserReviewForGameAsync("u1", Guid.NewGuid());

            // Assert
            result.Should().BeNull();
            _mapperMock.Verify(m => m.Map<ReviewDto>(It.IsAny<Review>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithValidGameAndNoExistingReview_PersistsAndReturnsDto()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = "user-123";
            var createDto = new CreateReviewDto { GameId = gameId, Rating = 4, Content = "Great" };
            var game = new Game { Id = gameId, Name = "Game" };
            _gameRepoMock.Setup(r => r.GetByIdWithDetailsAsync(gameId)).ReturnsAsync(game);
            _reviewRepoMock.Setup(r => r.GetUserReviewForGameAsync(userId, gameId)).ReturnsAsync((Review?)null);
            _reviewRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<Review>()))
                .ReturnsAsync((Review r) => r);
            _reviewRepoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) => new Review { Id = id, UserId = userId, GameId = gameId, Rating = 4, Content = "Great" });
            _mapperMock
                .Setup(m => m.Map<ReviewDto>(It.IsAny<Review>()))
                .Returns((Review r) => new ReviewDto { Id = r.Id, Rating = r.Rating, Content = r.Content });

            // Act
            var result = await _sut.CreateAsync(createDto, userId);

            // Assert
            result.Should().NotBeNull();
            result.Rating.Should().Be(4);
            _reviewRepoMock.Verify(r => r.CreateAsync(It.Is<Review>(rev =>
                rev.GameId == gameId && rev.UserId == userId && rev.Rating == 4 && rev.Content == "Great")),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithMissingGame_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new CreateReviewDto { GameId = Guid.NewGuid(), Rating = 5, Content = "x" };
            _gameRepoMock.Setup(r => r.GetByIdWithDetailsAsync(dto.GameId)).ReturnsAsync((Game?)null);

            // Act
            Func<Task> act = async () => await _sut.CreateAsync(dto, "u1");

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .Where(e => e.ResourceType == "Game");
            _reviewRepoMock.Verify(r => r.CreateAsync(It.IsAny<Review>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateReview_ThrowsConflictException()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = "u1";
            var dto = new CreateReviewDto { GameId = gameId, Rating = 5, Content = "x" };
            _gameRepoMock.Setup(r => r.GetByIdWithDetailsAsync(gameId)).ReturnsAsync(new Game { Id = gameId, Name = "g" });
            _reviewRepoMock
                .Setup(r => r.GetUserReviewForGameAsync(userId, gameId))
                .ReturnsAsync(new Review { Id = Guid.NewGuid(), UserId = userId, GameId = gameId });

            // Act
            Func<Task> act = async () => await _sut.CreateAsync(dto, userId);

            // Assert
            await act.Should().ThrowAsync<ConflictException>();
            _reviewRepoMock.Verify(r => r.CreateAsync(It.IsAny<Review>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_AsOwner_UpdatesReviewAndReturnsDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = "owner";
            var existing = new Review { Id = id, UserId = userId, GameId = Guid.NewGuid(), Rating = 1, Content = "old" };
            var update = new UpdateReviewDto { Rating = 5, Content = "new content" };
            _reviewRepoMock.SetupSequence(r => r.GetByIdAsync(id))
                .ReturnsAsync(existing) // first lookup
                .ReturnsAsync(existing); // reload for mapping
            _reviewRepoMock.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(existing);
            _mapperMock
                .Setup(m => m.Map<ReviewDto>(existing))
                .Returns(new ReviewDto { Id = id, Rating = 5, Content = "new content" });

            // Act
            var result = await _sut.UpdateAsync(id, update, userId);

            // Assert
            result.Rating.Should().Be(5);
            result.Content.Should().Be("new content");
            existing.Rating.Should().Be(5);
            existing.Content.Should().Be("new content");
        }

        [Fact]
        public async Task UpdateAsync_AsNonOwner_ThrowsForbiddenException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new Review { Id = id, UserId = "owner", GameId = Guid.NewGuid() };
            _reviewRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);

            // Act
            Func<Task> act = async () => await _sut.UpdateAsync(id, new UpdateReviewDto { Rating = 3, Content = "x" }, "intruder");

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
            _reviewRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Review>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithMissingReview_ThrowsNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _reviewRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Review?)null);

            // Act
            Func<Task> act = async () => await _sut.UpdateAsync(id, new UpdateReviewDto { Rating = 3, Content = "x" }, "u1");

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DeleteAsync_AsOwner_CallsRepositoryDelete()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new Review { Id = id, UserId = "owner" };
            _reviewRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
            _reviewRepoMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

            // Act
            await _sut.DeleteAsync(id, "owner");

            // Assert
            _reviewRepoMock.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_AsNonOwner_ThrowsForbiddenException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _reviewRepoMock
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(new Review { Id = id, UserId = "owner" });

            // Act
            Func<Task> act = async () => await _sut.DeleteAsync(id, "intruder");

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
            _reviewRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithMissingReview_ThrowsNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _reviewRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Review?)null);

            // Act
            Func<Task> act = async () => await _sut.DeleteAsync(id, "u1");

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UserCanReviewGameAsync_WhenGameMissing_ReturnsFalse()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _gameRepoMock.Setup(r => r.GetByIdWithDetailsAsync(gameId)).ReturnsAsync((Game?)null);

            // Act
            var result = await _sut.UserCanReviewGameAsync("u1", gameId);

            // Assert
            result.Should().BeFalse();
            _reviewRepoMock.Verify(r => r.GetUserReviewForGameAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UserCanReviewGameAsync_WhenUserAlreadyReviewed_ReturnsFalse()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _gameRepoMock.Setup(r => r.GetByIdWithDetailsAsync(gameId)).ReturnsAsync(new Game { Id = gameId, Name = "g" });
            _reviewRepoMock
                .Setup(r => r.GetUserReviewForGameAsync("u1", gameId))
                .ReturnsAsync(new Review { Id = Guid.NewGuid() });

            // Act
            var result = await _sut.UserCanReviewGameAsync("u1", gameId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserCanReviewGameAsync_WhenGameExistsAndNoExistingReview_ReturnsTrue()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _gameRepoMock.Setup(r => r.GetByIdWithDetailsAsync(gameId)).ReturnsAsync(new Game { Id = gameId, Name = "g" });
            _reviewRepoMock
                .Setup(r => r.GetUserReviewForGameAsync("u1", gameId))
                .ReturnsAsync((Review?)null);

            // Act
            var result = await _sut.UserCanReviewGameAsync("u1", gameId);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(3.5)]
        [InlineData(5.0)]
        public async Task GetAverageRatingForGameAsync_DelegatesToRepository(double expected)
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _reviewRepoMock.Setup(r => r.GetAverageRatingForGameAsync(gameId)).ReturnsAsync(expected);

            // Act
            var result = await _sut.GetAverageRatingForGameAsync(gameId);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public async Task GetPagedAsync_DelegatesAndMapsItems()
        {
            // Arrange
            var reviews = new List<Review> { new Review { Id = Guid.NewGuid(), UserId = "u1" } };
            var dtos = new List<ReviewDto> { new ReviewDto { Id = reviews[0].Id } };
            _reviewRepoMock
                .Setup(r => r.GetPagedAsync(2, 5, null, null))
                .ReturnsAsync((reviews.AsEnumerable(), 42));
            _mapperMock
                .Setup(m => m.Map<IEnumerable<ReviewDto>>(reviews))
                .Returns(dtos);

            // Act
            var (resultReviews, totalCount) = await _sut.GetPagedAsync(2, 5);

            // Assert
            totalCount.Should().Be(42);
            resultReviews.Should().BeEquivalentTo(dtos);
        }
    }
}
