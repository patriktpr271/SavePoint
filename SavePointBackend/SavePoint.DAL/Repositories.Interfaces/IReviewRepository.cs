using SavePoint.Common.Dtos.Reviews;
using SavePoint.Entities.Reviews;

namespace SavePoint.DAL.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetAllAsync();
        Task<Review?> GetByIdAsync(Guid id);
        Task<IEnumerable<Review>> GetByGameIdAsync(Guid gameId);
        Task<IEnumerable<Review>> GetByUserIdAsync(string userId);
        Task<Review?> GetUserReviewForGameAsync(string userId, Guid gameId);
        Task<Review> CreateAsync(Review review);
        Task<Review> UpdateAsync(Review review);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<double> GetAverageRatingForGameAsync(Guid gameId);
        Task<int> GetReviewCountForGameAsync(Guid gameId);
        Task<(IEnumerable<Review> Reviews, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Guid? gameId = null, string? userId = null);
    }
}