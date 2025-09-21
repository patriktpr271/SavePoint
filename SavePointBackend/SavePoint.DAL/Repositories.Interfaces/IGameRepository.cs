using SavePoint.Entities.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.DAL.Repositories.Interfaces
{
	public interface IGameRepository
	{
		Task InsertOrUpdateAsync(Game game);
		Task<Game?> GetByExternalIdAsync(long? externalId);
	}
}
