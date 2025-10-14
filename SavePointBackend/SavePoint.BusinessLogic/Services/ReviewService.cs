using AutoMapper;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.Common.Dtos.Reviews;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Reviews;

namespace SavePoint.BusinessLogic.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IMapper _mapper;

        public ReviewService(IReviewRepository reviewRepository, IGameRepository gameRepository, IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _gameRepository = gameRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReviewDto>> GetAllAsync()
        {
            var reviews = await _reviewRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ReviewDto?> GetByIdAsync(Guid id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            return review != null ? _mapper.Map<ReviewDto>(review) : null;
        }

        public async Task<IEnumerable<ReviewDto>> GetByGameIdAsync(Guid gameId)
        {
            var reviews = await _reviewRepository.GetByGameIdAsync(gameId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<IEnumerable<ReviewDto>> GetByUserIdAsync(string userId)
        {
            var reviews = await _reviewRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ReviewDto?> GetUserReviewForGameAsync(string userId, Guid gameId)
        {
            var review = await _reviewRepository.GetUserReviewForGameAsync(userId, gameId);
            return review != null ? _mapper.Map<ReviewDto>(review) : null;
        }

        public async Task<ReviewDto> CreateAsync(CreateReviewDto createDto, string userId)
        {
            // Check if game exists
            var game = await _gameRepository.GetByIdWithDetailsAsync(createDto.GameId);
            if (game == null)
                throw new ArgumentException("Game not found");

            // Check if user already has a review for this game
            var existingReview = await _reviewRepository.GetUserReviewForGameAsync(userId, createDto.GameId);
            if (existingReview != null)
                throw new InvalidOperationException("User already has a review for this game");

            var review = new Review
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GameId = createDto.GameId,
                Rating = createDto.Rating,
                Content = createDto.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdReview = await _reviewRepository.CreateAsync(review);
            
            // Reload with includes for mapping
            var reviewWithIncludes = await _reviewRepository.GetByIdAsync(createdReview.Id);
            return _mapper.Map<ReviewDto>(reviewWithIncludes);
        }

        public async Task<ReviewDto?> UpdateAsync(Guid id, UpdateReviewDto updateDto, string userId)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
                return null;

            // Check if the review belongs to the user
            if (review.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own reviews");

            review.Rating = updateDto.Rating;
            review.Content = updateDto.Content;
            review.UpdatedAt = DateTime.UtcNow;

            var updatedReview = await _reviewRepository.UpdateAsync(review);
            
            // Reload with includes for mapping
            var reviewWithIncludes = await _reviewRepository.GetByIdAsync(updatedReview.Id);
            return _mapper.Map<ReviewDto>(reviewWithIncludes);
        }

        public async Task<bool> DeleteAsync(Guid id, string userId)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
                return false;

            // Check if the review belongs to the user
            if (review.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own reviews");

            return await _reviewRepository.DeleteAsync(id);
        }

        public async Task<bool> UserCanReviewGameAsync(string userId, Guid gameId)
        {
            // Check if game exists
            var game = await _gameRepository.GetByIdWithDetailsAsync(gameId);
            if (game == null)
                return false;

            // Check if user already has a review for this game
            var existingReview = await _reviewRepository.GetUserReviewForGameAsync(userId, gameId);
            return existingReview == null;
        }

        public async Task<double> GetAverageRatingForGameAsync(Guid gameId)
        {
            return await _reviewRepository.GetAverageRatingForGameAsync(gameId);
        }

        public async Task<int> GetReviewCountForGameAsync(Guid gameId)
        {
            return await _reviewRepository.GetReviewCountForGameAsync(gameId);
        }

        public async Task<(IEnumerable<ReviewDto> Reviews, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Guid? gameId = null, string? userId = null)
        {
            var (reviews, totalCount) = await _reviewRepository.GetPagedAsync(pageNumber, pageSize, gameId, userId);
            var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
            return (reviewDtos, totalCount);
        }
    }
}