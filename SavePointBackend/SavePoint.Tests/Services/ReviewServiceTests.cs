using Moq;
using AutoMapper;
using FluentAssertions;
using SavePoint.BusinessLogic.Services;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Reviews;
using SavePoint.Entities.Games;
using SavePoint.Common.Dtos.Reviews;
using SavePoint.Common.Exceptions;

namespace SavePoint.Tests.Services
{
    public class ReviewServiceTests
    {
        private readonly Mock<IReviewRepository> _mockReviewRepository;
        private readonly Mock<IGameRepository> _mockGameRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ReviewService _reviewService;

        public ReviewServiceTests()
        {
            _mockReviewRepository = new Mock<IReviewRepository>();
            _mockGameRepository = new Mock<IGameRepository>();
            _mockMapper = new Mock<IMapper>();
            _reviewService = new ReviewService(_mockReviewRepository.Object, _mockGameRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllReviews()
        {
            // Arrange
            var reviews = new List<Review>
            {
                new Review { Id = Guid.NewGuid(), Rating = 5, Content = "Great game!" },
                new Review { Id = Guid.NewGuid(), Rating = 4, Content = "Good game" }
            };

            var reviewDtos = new List<ReviewDto>
            {
                new ReviewDto { Id = reviews[0].Id, Rating = 5, Content = "Great game!" },
                new ReviewDto { Id = reviews[1].Id, Rating = 4, Content = "Good game" }
            };

            _mockReviewRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(reviews);
            _mockMapper.Setup(x => x.Map<IEnumerable<ReviewDto>>(reviews)).Returns(reviewDtos);

            // Act
            var result = await _reviewService.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(reviewDtos);

            _mockReviewRepository.Verify(x => x.GetAllAsync(), Times.Once);
            _mockMapper.Verify(x => x.Map<IEnumerable<ReviewDto>>(reviews), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsReview()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var review = new Review { Id = reviewId, Rating = 5, Content = "Great game!" };
            var reviewDto = new ReviewDto { Id = reviewId, Rating = 5, Content = "Great game!" };

            _mockReviewRepository.Setup(x => x.GetByIdAsync(reviewId)).ReturnsAsync(review);
            _mockMapper.Setup(x => x.Map<ReviewDto>(review)).Returns(reviewDto);

            // Act
            var result = await _reviewService.GetByIdAsync(reviewId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(reviewDto);

            _mockReviewRepository.Verify(x => x.GetByIdAsync(reviewId), Times.Once);
            _mockMapper.Verify(x => x.Map<ReviewDto>(review), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ThrowsNotFoundException()
        {
            // Arrange
            var reviewId = Guid.NewGuid();

            _mockReviewRepository.Setup(x => x.GetByIdAsync(reviewId)).ReturnsAsync((Review?)null);

            // Act & Assert
            await _reviewService.Invoking(x => x.GetByIdAsync(reviewId))
                .Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Review not found with id: {reviewId}");

            _mockReviewRepository.Verify(x => x.GetByIdAsync(reviewId), Times.Once);
            _mockMapper.Verify(x => x.Map<ReviewDto>(It.IsAny<Review>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithValidData_CreatesReview()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = "test-user-id";
            var createDto = new CreateReviewDto
            {
                GameId = gameId,
                Rating = 5,
                Content = "Great game!"
            };

            var game = new Game { Id = gameId, Name = "Test Game" };
            var createdReview = new Review 
            { 
                Id = Guid.NewGuid(), 
                UserId = userId, 
                GameId = gameId, 
                Rating = 5, 
                Content = "Great game!" 
            };
            var reviewDto = new ReviewDto 
            { 
                Id = createdReview.Id, 
                UserId = userId, 
                GameId = gameId, 
                Rating = 5, 
                Content = "Great game!" 
            };

            _mockGameRepository.Setup(x => x.GetByIdWithDetailsAsync(gameId)).ReturnsAsync(game);
            _mockReviewRepository.Setup(x => x.GetUserReviewForGameAsync(userId, gameId)).ReturnsAsync((Review?)null);
            _mockReviewRepository.Setup(x => x.CreateAsync(It.IsAny<Review>())).ReturnsAsync(createdReview);
            _mockReviewRepository.Setup(x => x.GetByIdAsync(createdReview.Id)).ReturnsAsync(createdReview);
            _mockMapper.Setup(x => x.Map<ReviewDto>(createdReview)).Returns(reviewDto);

            // Act
            var result = await _reviewService.CreateAsync(createDto, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(reviewDto);

            _mockGameRepository.Verify(x => x.GetByIdWithDetailsAsync(gameId), Times.Once);
            _mockReviewRepository.Verify(x => x.GetUserReviewForGameAsync(userId, gameId), Times.Once);
            _mockReviewRepository.Verify(x => x.CreateAsync(It.Is<Review>(r => 
                r.UserId == userId && 
                r.GameId == gameId && 
                r.Rating == 5 && 
                r.Content == "Great game!")), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNonExistentGame_ThrowsNotFoundException()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = "test-user-id";
            var createDto = new CreateReviewDto
            {
                GameId = gameId,
                Rating = 5,
                Content = "Great game!"
            };

            _mockGameRepository.Setup(x => x.GetByIdWithDetailsAsync(gameId)).ReturnsAsync((Game?)null);

            // Act & Assert
            await _reviewService.Invoking(x => x.CreateAsync(createDto, userId))
                .Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Game not found with id: {gameId}");

            _mockGameRepository.Verify(x => x.GetByIdWithDetailsAsync(gameId), Times.Once);
            _mockReviewRepository.Verify(x => x.CreateAsync(It.IsAny<Review>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithExistingReview_ThrowsConflictException()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = "test-user-id";
            var createDto = new CreateReviewDto
            {
                GameId = gameId,
                Rating = 5,
                Content = "Great game!"
            };

            var game = new Game { Id = gameId, Name = "Test Game" };
            var existingReview = new Review { Id = Guid.NewGuid(), UserId = userId, GameId = gameId };

            _mockGameRepository.Setup(x => x.GetByIdWithDetailsAsync(gameId)).ReturnsAsync(game);
            _mockReviewRepository.Setup(x => x.GetUserReviewForGameAsync(userId, gameId)).ReturnsAsync(existingReview);

            // Act & Assert
            await _reviewService.Invoking(x => x.CreateAsync(createDto, userId))
                .Should().ThrowAsync<ConflictException>();

            _mockGameRepository.Verify(x => x.GetByIdWithDetailsAsync(gameId), Times.Once);
            _mockReviewRepository.Verify(x => x.GetUserReviewForGameAsync(userId, gameId), Times.Once);
            _mockReviewRepository.Verify(x => x.CreateAsync(It.IsAny<Review>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithValidData_UpdatesReview()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = "test-user-id";
            var updateDto = new UpdateReviewDto
            {
                Rating = 4,
                Content = "Updated content"
            };

            var existingReview = new Review 
            { 
                Id = reviewId, 
                UserId = userId, 
                Rating = 5, 
                Content = "Original content" 
            };

            var reviewDto = new ReviewDto 
            { 
                Id = reviewId, 
                UserId = userId, 
                Rating = 4, 
                Content = "Updated content" 
            };

            // Setup the first call to return the existing review for validation
            _mockReviewRepository.SetupSequence(x => x.GetByIdAsync(reviewId))
                .ReturnsAsync(existingReview)
                .ReturnsAsync(existingReview); // Second call for reload

            _mockReviewRepository.Setup(x => x.UpdateAsync(It.IsAny<Review>()))
                .ReturnsAsync(existingReview); // Return the updated review

            _mockMapper.Setup(x => x.Map<ReviewDto>(existingReview)).Returns(reviewDto);

            // Act
            var result = await _reviewService.UpdateAsync(reviewId, updateDto, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(reviewDto);

            _mockReviewRepository.Verify(x => x.GetByIdAsync(reviewId), Times.Exactly(2)); // Once for validation, once for final mapping
            _mockReviewRepository.Verify(x => x.UpdateAsync(It.Is<Review>(r => 
                r.Id == reviewId && 
                r.Rating == 4 && 
                r.Content == "Updated content")), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentReview_ThrowsNotFoundException()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var userId = "test-user-id";
            var updateDto = new UpdateReviewDto { Rating = 4, Content = "Updated content" };

            _mockReviewRepository.Setup(x => x.GetByIdAsync(reviewId)).ReturnsAsync((Review?)null);

            // Act & Assert
            await _reviewService.Invoking(x => x.UpdateAsync(reviewId, updateDto, userId))
                .Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Review not found with id: {reviewId}");

            _mockReviewRepository.Verify(x => x.GetByIdAsync(reviewId), Times.Once);
            _mockReviewRepository.Verify(x => x.UpdateAsync(It.IsAny<Review>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithDifferentUser_ThrowsForbiddenException()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var originalUserId = "original-user-id";
            var attemptingUserId = "different-user-id";
            var updateDto = new UpdateReviewDto { Rating = 4, Content = "Updated content" };

            var existingReview = new Review 
            { 
                Id = reviewId, 
                UserId = originalUserId, 
                Rating = 5, 
                Content = "Original content" 
            };

            _mockReviewRepository.Setup(x => x.GetByIdAsync(reviewId)).ReturnsAsync(existingReview);

            // Act & Assert
            await _reviewService.Invoking(x => x.UpdateAsync(reviewId, updateDto, attemptingUserId))
                .Should().ThrowAsync<ForbiddenException>()
                .WithMessage("You do not have permission to update review");

            _mockReviewRepository.Verify(x => x.GetByIdAsync(reviewId), Times.Once);
            _mockReviewRepository.Verify(x => x.UpdateAsync(It.IsAny<Review>()), Times.Never);
        }

        [Fact]
        public async Task UserCanReviewGameAsync_WithValidGame_ReturnsTrue()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = "test-user-id";
            var game = new Game { Id = gameId, Name = "Test Game" };

            _mockGameRepository.Setup(x => x.GetByIdWithDetailsAsync(gameId)).ReturnsAsync(game);
            _mockReviewRepository.Setup(x => x.GetUserReviewForGameAsync(userId, gameId)).ReturnsAsync((Review?)null);

            // Act
            var result = await _reviewService.UserCanReviewGameAsync(userId, gameId);

            // Assert
            result.Should().BeTrue();

            _mockGameRepository.Verify(x => x.GetByIdWithDetailsAsync(gameId), Times.Once);
            _mockReviewRepository.Verify(x => x.GetUserReviewForGameAsync(userId, gameId), Times.Once);
        }

        [Fact]
        public async Task UserCanReviewGameAsync_WithExistingReview_ReturnsFalse()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = "test-user-id";
            var game = new Game { Id = gameId, Name = "Test Game" };
            var existingReview = new Review { Id = Guid.NewGuid(), UserId = userId, GameId = gameId };

            _mockGameRepository.Setup(x => x.GetByIdWithDetailsAsync(gameId)).ReturnsAsync(game);
            _mockReviewRepository.Setup(x => x.GetUserReviewForGameAsync(userId, gameId)).ReturnsAsync(existingReview);

            // Act
            var result = await _reviewService.UserCanReviewGameAsync(userId, gameId);

            // Assert
            result.Should().BeFalse();

            _mockGameRepository.Verify(x => x.GetByIdWithDetailsAsync(gameId), Times.Once);
            _mockReviewRepository.Verify(x => x.GetUserReviewForGameAsync(userId, gameId), Times.Once);
        }

        [Fact]
        public async Task GetAverageRatingForGameAsync_ReturnsCorrectAverage()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var expectedAverage = 4.5;

            _mockReviewRepository.Setup(x => x.GetAverageRatingForGameAsync(gameId)).ReturnsAsync(expectedAverage);

            // Act
            var result = await _reviewService.GetAverageRatingForGameAsync(gameId);

            // Assert
            result.Should().Be(expectedAverage);
            _mockReviewRepository.Verify(x => x.GetAverageRatingForGameAsync(gameId), Times.Once);
        }

        [Fact]
        public async Task GetReviewCountForGameAsync_ReturnsCorrectCount()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var expectedCount = 25;

            _mockReviewRepository.Setup(x => x.GetReviewCountForGameAsync(gameId)).ReturnsAsync(expectedCount);

            // Act
            var result = await _reviewService.GetReviewCountForGameAsync(gameId);

            // Assert
            result.Should().Be(expectedCount);
            _mockReviewRepository.Verify(x => x.GetReviewCountForGameAsync(gameId), Times.Once);
        }
    }
}