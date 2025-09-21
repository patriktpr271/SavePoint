using IGDB;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Games;
using SavePoint.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.BusinessLogic.Services
{
	public class IGDBImportService : IIGDBImportService
	{
		private readonly IGDBClient _client;
		private readonly IGenreRepository _genreRepository;
		private readonly IGameRepository _gameRepository;
		private readonly ICompanyRepository _companyRepository;
		private readonly IPlatfromRepository _platformRepository;

		public IGDBImportService(IGDBClient client, IGenreRepository genreRepository, IGameRepository gameRepository, ICompanyRepository companyRepository, IPlatfromRepository platfromRepository)
		{
			_client = client;
			_genreRepository = genreRepository;
			_gameRepository = gameRepository;
			_companyRepository = companyRepository;
			_platformRepository = platfromRepository;
		}

		public async Task ImportGamesAsync()
		{
			var genreList = await _genreRepository.GetAllAsync();
			var platformList = await _platformRepository.GetAllAsync();
			var companyList = await _companyRepository.GetAllAsync();

			var games = await _client.QueryAsync<IGDB.Models.Game>(
				 IGDBClient.Endpoints.Games,
				 "fields id,name,summary,cover.*,first_release_date,genres,platforms,involved_companies.company,involved_companies.developer,involved_companies.publisher; limit 500; offset 1000;"
				);
			foreach (var game in games)
			{
				if (game != null)
				{
					var coverUrl = game.Cover?.Value?.Url;
					var genresExternalIds = game.Genres?.Ids;
					var platformsExternalIds = game.Platforms?.Ids;

					// Map IGDB genre IDs to internal GUIDs
					List<GameGenre> gameGenres = new();
					if (genresExternalIds != null && genresExternalIds.Any())
					{
						var internalGenres = genreList.Where(g => genresExternalIds.Contains((long)g.ExternalId)).ToList();
						foreach (var genre in internalGenres)
						{
							gameGenres.Add(new GameGenre
							{
								GenreId = genre.Id,
								Id = Guid.NewGuid()
							});
						}
					}

					//Map IGDB platform IDs to internal GUIDs
					List<GamePlatform> gamePlatforms = new();
					if (platformsExternalIds != null && platformsExternalIds.Any())
					{
						var internalPlatforms = platformList.Where(p => platformsExternalIds.Contains((long)p.ExternalId)).ToList();
						foreach (var platform in internalPlatforms)
						{
							gamePlatforms.Add(new GamePlatform
							{
								PlatformId = platform.Id,
								Id = Guid.NewGuid()
							});
						}
					}

					// Map IGDB company relationships
					List<GameCompany> gameCompanies = new();
					if (game.InvolvedCompanies != null)
					{
						foreach (var involvedCompany in game.InvolvedCompanies.Values)
						{
							var company = companyList.FirstOrDefault(c => c.ExternalId == involvedCompany.Company.Id);
							if (company != null)
							{
								// Add as developer if flagged as developer
								if (involvedCompany.Developer == true)
								{
									gameCompanies.Add(new GameCompany
									{
										Id = Guid.NewGuid(),
										CompanyId = company.Id,
										Role = CompanyRole.Developer
									});
								}

								// Add as publisher if flagged publisher
								if (involvedCompany.Publisher == true)
								{
									gameCompanies.Add(new GameCompany
									{
										Id = Guid.NewGuid(),
										CompanyId = company.Id,
										Role = CompanyRole.Publisher
									});
								}
							}
						}
					}

					//create game entity
					var gameEntity = new Game
					{
						Id = Guid.NewGuid(),
						ExternalId = game.Id,
						Name = game.Name,
						Summary = game.Summary,
						CoverUrl = coverUrl,
						ReleaseDate = game.FirstReleaseDate?.UtcDateTime ?? DateTime.MinValue,
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow,
						GameGenres = gameGenres,
						GamePlatforms = gamePlatforms,
						GameCompanies = gameCompanies
					};

					// Set the GameId for all related entities
					gameGenres.ForEach(gg => gg.GameId = gameEntity.Id);
					gamePlatforms.ForEach(gp => gp.GameId = gameEntity.Id);
					gameCompanies.ForEach(gc => gc.GameId = gameEntity.Id);

					await _gameRepository.InsertOrUpdateAsync(gameEntity);
				}
			}			
			
		}

		public async Task ImportGenresAsync()
		{
			int limit = 500;
			int offset = 0;
			bool hasMore = true;

			while (hasMore)
			{
				var response = await _client.QueryAsync<IGDB.Models.Genre>(
					IGDBClient.Endpoints.Genres,
					$"fields id,name; limit {limit}; offset {offset};"
				);

				if (response.Count() == 0)
				{
					hasMore = false;
					break;
				}

				foreach (var g in response)
				{
					var genre = new Genre
					{
						Id = Guid.NewGuid(),
						ExternalId = g.Id,
						Name = g.Name,
						CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
					};

					await _genreRepository.InsertOrUpdateAsync(genre);
				}

				offset += limit;
			}
		}

		public async Task ImportCompaniesAsync()
		{
			int limit = 500;
			int offset = 0;
			bool hasMore = true;
			var companyList = new List<Company>();

			while (hasMore)
			{
				var result = await _client.QueryAsync<IGDB.Models.Company>(
					IGDBClient.Endpoints.Companies,
					$"fields id,name,description,logo.*; limit {limit}; offset {offset};"
				);

				offset += limit;

				if (result.Count() == 0)
				{
					hasMore = false;
					break;
				}
				foreach (var c in result)
				{
					var company = new Company
					{
						Id = Guid.NewGuid(),
						ExternalId = c.Id,
						Name = c.Name,
					};

					companyList.Add(company);
				}
			}

			await _companyRepository.InsertOrUpdateAsyncBatch(companyList);
		}

		public async Task ImportPlatformsAsync()
		{
			int limit = 500;
			int offset = 0;
			bool hasMore = true;
			var platformList = new List<Platform>();

			while (hasMore)
			{
				var result = await _client.QueryAsync<IGDB.Models.Platform>(
					IGDBClient.Endpoints.Platforms,
					$"fields id,name,abbreviation; limit {limit}; offset {offset};"
				);
				offset += limit;
				if (result.Count() == 0)
				{
					hasMore = false;
					break;
				}

				foreach (var p in result)
				{
					var platform = new Platform
					{
						Id = Guid.NewGuid(),
						ExternalId = p.Id,
						Name = p.Name,
						Abbreviation = p.Abbreviation
					};
					platformList.Add(platform);
				}
			}

			await _platformRepository.InsertOrUpdateAsyncBatch(platformList);
		}

		/// <summary>
		/// Import everything in the correct order
		/// </summary>
		public async Task ImportAllDataAsync()
		{
			// Step 1: Import base entities first (no dependencies)
			await ImportGenresAsync();
			await ImportPlatformsAsync();
			await ImportCompaniesAsync();

			// Step 2: Import games with relationships
			await ImportGamesAsync();
		}
	}
}
