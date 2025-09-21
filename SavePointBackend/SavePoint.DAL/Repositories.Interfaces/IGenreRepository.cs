using SavePoint.Entities.Games;

namespace SavePoint.DAL.Repositories.Interfaces
{
	public interface IGenreRepository
	{
		Task InsertOrUpdateAsync(Genre genre);
		Task<List<Genre>> GetByExternalIds(IEnumerable<long> externalIds);

		Task<List<Genre>> GetAllAsync();
	}
}
