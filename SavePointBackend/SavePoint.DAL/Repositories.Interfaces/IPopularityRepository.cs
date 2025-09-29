using SavePoint.Entities.Popularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.DAL.Repositories.Interfaces
{
	public interface IPopularityRepository
	{
		Task InsertOrUpdateAsyncBatch(List<Popularity> popularities);
		Task<List<Popularity>> GetByGameIdAsync(Guid gameId);
		Task<List<Popularity>> GetAllAsync();
	}
}
