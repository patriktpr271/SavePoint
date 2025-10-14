using Microsoft.EntityFrameworkCore;
using SavePoint.DAL.Contexts;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Reviews;

namespace SavePoint.DAL.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Game)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetByIdAsync(Guid id)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Game)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Review>> GetByGameIdAsync(Guid gameId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Game)
                .Where(r => r.GameId == gameId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByUserIdAsync(string userId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Game)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetUserReviewForGameAsync(string userId, Guid gameId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Game)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.GameId == gameId);
        }

        public async Task<Review> CreateAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<Review> UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Reviews.AnyAsync(r => r.Id == id);
        }

        public async Task<double> GetAverageRatingForGameAsync(Guid gameId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.GameId == gameId)
                .ToListAsync();

            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }

        public async Task<int> GetReviewCountForGameAsync(Guid gameId)
        {
            return await _context.Reviews
                .CountAsync(r => r.GameId == gameId);
        }

        public async Task<(IEnumerable<Review> Reviews, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Guid? gameId = null, string? userId = null)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Game)
                .AsQueryable();

            if (gameId.HasValue)
                query = query.Where(r => r.GameId == gameId.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(r => r.UserId == userId);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }
    }
}