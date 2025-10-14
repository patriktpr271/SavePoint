using SavePoint.Common.Dtos.Reviews;

namespace SavePoint.BusinessLogic.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetAllAsync();
        Task<ReviewDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<ReviewDto>> GetByGameIdAsync(Guid gameId);
        Task<IEnumerable<ReviewDto>> GetByUserIdAsync(string userId);
        Task<ReviewDto?> GetUserReviewForGameAsync(string userId, Guid gameId);
        Task<ReviewDto> CreateAsync(CreateReviewDto createDto, string userId);
        Task<ReviewDto?> UpdateAsync(Guid id, UpdateReviewDto updateDto, string userId);
        Task<bool> DeleteAsync(Guid id, string userId);
        Task<bool> UserCanReviewGameAsync(string userId, Guid gameId);
        Task<double> GetAverageRatingForGameAsync(Guid gameId);
        Task<int> GetReviewCountForGameAsync(Guid gameId);
        Task<(IEnumerable<ReviewDto> Reviews, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Guid? gameId = null, string? userId = null);
    }
}