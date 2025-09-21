using SavePoint.Entities.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.DAL.Repositories.Interfaces
{
	public interface IPlatfromRepository
	{
		Task InsertOrUpdateAsyncBatch(List<Platform> platforms);
		Task<List<Platform>> GetByExternalIds(IEnumerable<long> externalIds);
		Task<List<Platform>> GetAllAsync();
	}
}
