using SavePoint.Entities.Common;
using SavePoint.Common.Dtos.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.BusinessLogic.Services.Interfaces
{
	public interface IGameService
	{
		Task<PagedResult<GameCardDto>> GetPopularGames(int popularityType, int pageNumber = 1, int pageSize = 20);
		Task<GameDetailDto?> GetGameById(Guid id);
	}
}
