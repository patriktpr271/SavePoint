using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Common;
using SavePoint.Common.Dtos.Games;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.BusinessLogic.Services
{
	public class GameService : IGameService
	{
		private readonly IGameRepository _gameRepository;
		private readonly IMapper _mapper;

		public GameService(IGameRepository gameRepository, IMapper mapper)
		{
			_gameRepository = gameRepository;
			_mapper = mapper;
		}

		public async Task<PagedResult<GameCardDto>> GetPopularGames(int popularityType, int pageNumber = 1, int pageSize = 20)
		{
			var gameResult = await _gameRepository.GetPopularGames(popularityType, pageNumber, pageSize);
			
			var gameCardDtos = _mapper.Map<List<GameCardDto>>(gameResult.Items);
			
			// Add popularity scores manually since they're context-specific
			foreach (var dto in gameCardDtos)
			{
				var game = gameResult.Items.First(g => g.Id == dto.Id);
				dto.PopularityScore = game.Popularities
					.Where(p => p.PopularityType == popularityType)
					.FirstOrDefault()?.PopularityScore;
			}

			return new PagedResult<GameCardDto>
			{
				Items = gameCardDtos,
				TotalCount = gameResult.TotalCount,
				PageNumber = gameResult.PageNumber,
				PageSize = gameResult.PageSize
			};
		}

		public async Task<GameDetailDto?> GetGameById(Guid id)
		{
			var game = await _gameRepository.GetByIdWithDetailsAsync(id);
			if (game == null)
				return null;

			var gameDetailDto = _mapper.Map<GameDetailDto>(game);
			
			// Add popularity score from the highest priority popularity type available
			var topPopularity = game.Popularities.OrderByDescending(p => p.PopularityScore).FirstOrDefault();
			gameDetailDto.PopularityScore = topPopularity?.PopularityScore;

			return gameDetailDto;
		}
	}
}
