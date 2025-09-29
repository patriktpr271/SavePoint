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
using SavePoint.Entities.Popularity;

namespace SavePoint.BusinessLogic.Services
{
	public class IGDBImportService : IIGDBImportService
	{
		private readonly IGDBClient _client;
		private readonly IGenreRepository _genreRepository;
		private readonly IGameRepository _gameRepository;
		private readonly ICompanyRepository _companyRepository;
		private readonly IPlatfromRepository _platformRepository;
		private readonly IPopularityRepository _popularityRepository;

		public IGDBImportService(IGDBClient client, IGenreRepository genreRepository, IGameRepository gameRepository, 
			ICompanyRepository companyRepository, IPlatfromRepository platfromRepository, IPopularityRepository popularityRepository)
		{
			_client = client;
			_genreRepository = genreRepository;
			_gameRepository = gameRepository;
			_companyRepository = companyRepository;
			_platformRepository = platfromRepository;
			_popularityRepository = popularityRepository;
		}

		/*public async Task ImportGamesAsync()
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
			
		}*/

		public async Task ImportGamesAsync()
		{
			var genreList = await _genreRepository.GetAllAsync();
			var platformList = await _platformRepository.GetAllAsync();
			var companyList = await _companyRepository.GetAllAsync();

			int limit = 500;
			int offset = 0;
			bool hasMore = true;


				var games = await _client.QueryAsync<IGDB.Models.Game>(
					 IGDBClient.Endpoints.Games,
					 $"fields id,name,summary,cover.*,first_release_date,genres,platforms,involved_companies.company,involved_companies.developer,involved_companies.publisher; limit {limit}; offset {offset};"
					);

				if (games.Count() == 0)
				{
					hasMore = false;
				}

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

				offset += limit;
			
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

		public async Task ImportPopularityAsync()
		{
			int limit = 500;
			int[] popularityTypes = new int[] { 1, 2, 5 };
			var allGames = await _gameRepository.GetAllAsync();
			var gameExternalIdMap = allGames.ToDictionary(g => g.ExternalId, g => g.Id);

			foreach (var popularityType in popularityTypes)
			{
				int offset = 0;
				bool hasMore = true;
				var popularityList = new List<Popularity>();

				while (hasMore)
				{
					var result = await _client.QueryAsync<IGDB.Models.PopularityPrimitive>(
						IGDBClient.Endpoints.PopularityPrimitives,
						$"fields game_id,value,popularity_type; limit {limit}; offset {offset}; where popularity_type = {popularityType};"
					);

					if (result.Count() == 0)
					{
						hasMore = false;
						break;
					}

					foreach (var p in result)
					{
						// Only create popularity records for games we have imported
						if (gameExternalIdMap.TryGetValue((long)p.GameId, out var gameId))
						{
							var popularity = new Popularity
							{
								Id = Guid.NewGuid(),
								ExternalGameId = (long)p.GameId,
								GameId = gameId, 
								PopularityScore = (decimal)p.Value,
								PopularityType = (long)p.PopularityType.Id,
								CreatedAt = DateTime.UtcNow,
								UpdatedAt = DateTime.UtcNow
							};
							popularityList.Add(popularity);
						}
					}

					// Save all popularity records for this batch
					if (popularityList.Any())
					{
						await _popularityRepository.InsertOrUpdateAsyncBatch(popularityList);
					}
					offset += limit;
				}

			
			}
		}

		public async Task ImportGamesWithPopularityAsync()
		{
			var genreList = await _genreRepository.GetAllAsync();
			var platformList = await _platformRepository.GetAllAsync();
			var companyList = await _companyRepository.GetAllAsync();

			int limit = 500;
			int offset = 0;
			bool hasMore = true;

			while (hasMore)
			{
				// Import games with popularity data included in the query
				var games = await _client.QueryAsync<IGDB.Models.Game>(
					 IGDBClient.Endpoints.Games,
					 $"fields id,name,summary,cover.*,first_release_date,genres,platforms,involved_companies.company,involved_companies.developer,involved_companies.publisher; limit {limit}; offset {offset};"
					);

				if (games.Count() == 0)
				{
					hasMore = false;
					break;
				}

				// Get game IDs for this batch to fetch their popularity data
				var gameIds = games.Select(g => g.Id).ToArray();
				var gameIdString = string.Join(",", gameIds);

				// Fetch popularity data for these specific games
				var popularityData = new List<IGDB.Models.PopularityPrimitive>();
				int[] popularityTypes = new int[] { 1, 2, 5 };

				foreach (var popularityType in popularityTypes)
				{
					var popularityResult = await _client.QueryAsync<IGDB.Models.PopularityPrimitive>(
						IGDBClient.Endpoints.PopularityPrimitives,
						$"fields game_id,value,popularity_type; where game_id = ({gameIdString}) & popularity_type = {popularityType};"
					);
					popularityData.AddRange(popularityResult);
				}

				// Group popularity data by game ID
				var popularityByGame = popularityData.GroupBy(p => p.GameId).ToDictionary(g => g.Key, g => g.ToList());

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

						// Map popularity data for this game
						List<Popularity> gamePopularities = new();
						if (popularityByGame.TryGetValue(game.Id, out var gamePopularityData))
						{
							foreach (var popularity in gamePopularityData)
							{
								gamePopularities.Add(new Popularity
								{
									Id = Guid.NewGuid(),
									ExternalGameId = (long)popularity.GameId,
									PopularityScore = (decimal)popularity.Value,
									PopularityType = (long)popularity.PopularityType.Id,
									CreatedAt = DateTime.UtcNow,
									UpdatedAt = DateTime.UtcNow
								});
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
							GameCompanies = gameCompanies,
							Popularities = gamePopularities
						};

						// Set the GameId for all related entities
						gameGenres.ForEach(gg => gg.GameId = gameEntity.Id);
						gamePlatforms.ForEach(gp => gp.GameId = gameEntity.Id);
						gameCompanies.ForEach(gc => gc.GameId = gameEntity.Id);
						gamePopularities.ForEach(gp => gp.GameId = gameEntity.Id);

						await _gameRepository.InsertOrUpdateAsync(gameEntity);
					}
				}

				offset += limit;
			}
		}

		/*public async Task ImportPopularityAsync()
		{
			int limit = 500;
			int offset = 0; ;
			bool hasMore = true;
			int[] popularityTypes = new int[] { 1, 2, 5}; 

			var popularityList = new List<Popularity>();

			foreach (var popularityType in popularityTypes)
			{
				while (hasMore)
				{
					var result = await _client.QueryAsync<IGDB.Models.PopularityPrimitive>(
						IGDBClient.Endpoints.PopularityPrimitives,
						$"fields game_id,value,popularity_type; limit {limit}; offset {offset}; where popularity_type = {popularityType};"
					);

					offset += limit;

					if (result.Count() == 0)
					{
						hasMore = false;
						break;
					}

					foreach (var p in result)
					{
						var popularity = new Popularity
						{
							Id = Guid.NewGuid(),
							ExternalGameId = (long)p.GameId,
							PopularityScore = (decimal)p.Value,
							PopularityType = (long)p.PopularityType.Id,
							CreatedAt = DateTime.UtcNow,
							UpdatedAt = DateTime.UtcNow
						};
						popularityList.Add(popularity);
					}
				}
			}
		
		}*/

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

			// Step 3: Import popularity data and link to existing games
			await ImportPopularityAsync();
		}

		/// <summary>
		/// Syncs popularity data for all existing games in the database
		/// </summary>
		public async Task SyncPopularityForExistingGamesAsync()
		{
			// Get all games from database
			var allGames = await _gameRepository.GetAllAsync();
			var gameExternalIdMap = allGames.ToDictionary(g => g.ExternalId, g => g.Id);

			Console.WriteLine($"Found {allGames.Count} games in database. Starting popularity sync...");

			int limit = 500;
			int[] popularityTypes = new int[] { 1, 2, 5 };
			int totalProcessed = 0;

			// Process games in batches to avoid memory issues and API rate limits
			int batchSize = 500;
			var gameExternalIds = allGames.Select(g => g.ExternalId).ToList();

			for (int batchIndex = 0; batchIndex < gameExternalIds.Count; batchIndex += batchSize)
			{
				var gameBatch = gameExternalIds.Skip(batchIndex).Take(batchSize).ToList();
				var gameIdString = string.Join(",", gameBatch);

				Console.WriteLine($"Processing batch {(batchIndex / batchSize) + 1}/{(gameExternalIds.Count + batchSize - 1) / batchSize} ({gameBatch.Count} games)");

				foreach (var popularityType in popularityTypes)
				{
					int offset = 0;
					bool hasMore = true;
					var popularityList = new List<Popularity>();

					while (hasMore)
					{
						try
						{
							var result = await _client.QueryAsync<IGDB.Models.PopularityPrimitive>(
								IGDBClient.Endpoints.PopularityPrimitives,
								$"fields game_id,value,popularity_type; limit {limit}; offset {offset}; where game_id = ({gameIdString}) & popularity_type = {popularityType};"
							);

							if (result.Count() == 0)
							{
								hasMore = false;
								break;
							}

							foreach (var p in result)
							{
								// Only create popularity records for games we have in our database
								if (gameExternalIdMap.TryGetValue((long)p.GameId, out var gameId))
								{
									var popularity = new Popularity
									{
										Id = Guid.NewGuid(),
										ExternalGameId = (long)p.GameId,
										GameId = gameId,
										PopularityScore = (decimal)p.Value,
										PopularityType = (long)p.PopularityType.Id,
										CreatedAt = DateTime.UtcNow,
										UpdatedAt = DateTime.UtcNow
									};
									popularityList.Add(popularity);
								}
							}


							// Save all popularity records for this type and batch
							if (popularityList.Any())
							{
								try
								{
									await _popularityRepository.InsertOrUpdateAsyncBatch(popularityList);
									totalProcessed += popularityList.Count;
									Console.WriteLine($"Saved {popularityList.Count} popularity records for type {popularityType}");
								}
								catch (Exception ex)
								{
									Console.WriteLine($"Error saving popularity data for type {popularityType}: {ex.Message}");
								}
							}

							offset += limit;
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error fetching popularity data for type {popularityType}, offset {offset}: {ex.Message}");
							// Continue with next batch instead of failing completely
							hasMore = false;
						}
					}

				}
			}

			Console.WriteLine($"Popularity sync completed. Total records processed: {totalProcessed}");
		}
		/// <summary>
		/// Imports popularity data for the three specified types (1, 2, 5) for games already in the database
		/// </summary>
		public async Task ImportPopularityForExistingGamesAsync()
		{
			// Get all games from database
			var allGames = await _gameRepository.GetAllAsync();
			var gameExternalIdMap = allGames.ToDictionary(g => g.ExternalId, g => g.Id);

			Console.WriteLine($"Found {allGames.Count} games in database. Starting popularity import...");

			int limit = 500;
			int[] popularityTypes = new int[] { 1, 2, 5 };
			int totalProcessed = 0;

			// Process each popularity type separately
			foreach (var popularityType in popularityTypes)
			{
				Console.WriteLine($"Processing popularity type {popularityType}...");

				int offset = 0;
				bool hasMore = true;

				while (hasMore)
				{
					try
					{
						var result = await _client.QueryAsync<IGDB.Models.PopularityPrimitive>(
							IGDBClient.Endpoints.PopularityPrimitives,
							$"fields game_id,value,popularity_type; limit {limit}; offset {offset}; where popularity_type = {popularityType};"
						);

						if (result.Count() == 0)
						{
							hasMore = false;
							break;
						}

						var popularityList = new List<Popularity>();

						foreach (var p in result)
						{
							// Only create popularity records for games we have in our database
							if (gameExternalIdMap.TryGetValue((long)p.GameId, out var gameId))
							{
								var popularity = new Popularity
								{
									Id = Guid.NewGuid(),
									ExternalGameId = (long)p.GameId,
									GameId = gameId,
									PopularityScore = (decimal)p.Value,
									PopularityType = (long)p.PopularityType.Id,
									CreatedAt = DateTime.UtcNow,
									UpdatedAt = DateTime.UtcNow
								};
								popularityList.Add(popularity);
							}
						}

						// Save all popularity records for this batch
						if (popularityList.Any())
						{
							try
							{
								await _popularityRepository.InsertOrUpdateAsyncBatch(popularityList);
								totalProcessed += popularityList.Count;
								Console.WriteLine($"Saved {popularityList.Count} popularity records for type {popularityType}, offset {offset}");
							}
							catch (Exception ex)
							{
								Console.WriteLine($"Error saving popularity data for type {popularityType}, offset {offset}: {ex.Message}");
							}
						}

						offset += limit;
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error fetching popularity data for type {popularityType}, offset {offset}: {ex.Message}");
						// Continue with next batch instead of failing completely
						hasMore = false;
					}
				}

				Console.WriteLine($"Completed popularity type {popularityType}");
			}

			Console.WriteLine($"Popularity import completed. Total records processed: {totalProcessed}");
		}

		public async Task ImportGamesWithBatchedPopularityAsync()
		{
			var genreList = await _genreRepository.GetAllAsync();
			var platformList = await _platformRepository.GetAllAsync();
			var companyList = await _companyRepository.GetAllAsync();

			int limit = 500;
			int offset = 0;
			int maxTotalGames = 5000; // Maximum number of games to import
			int totalGamesImported = 0; // Track total games imported
			bool hasMore = true;
			int[] popularityTypes = new int[] { 1, 2, 5 };

			Console.WriteLine($"Starting batched game import with immediate popularity sync (max {maxTotalGames} games)...");

			while (hasMore && totalGamesImported < maxTotalGames)
			{
				// Calculate how many games we can still import in this batch
				int remainingGames = maxTotalGames - totalGamesImported;
				int currentBatchLimit = Math.Min(limit, remainingGames);

				Console.WriteLine($"Processing batch starting at offset {offset} (importing up to {currentBatchLimit} games)...");

				// Step 1: Import games (up to the remaining limit)
				var games = await _client.QueryAsync<IGDB.Models.Game>(
					 IGDBClient.Endpoints.Games,
					 $"fields id,name,summary,cover.*,first_release_date,genres,platforms,involved_companies.company,involved_companies.developer,involved_companies.publisher; limit {currentBatchLimit}; offset {offset};"
					);

				if (games.Count() == 0)
				{
					hasMore = false;
					break;
				}

				// Step 2: Process and save games
				var importedGameIds = new List<long>();
				var gameExternalIdToInternalId = new Dictionary<long, Guid>();

				foreach (var game in games)
				{
					if (game != null && totalGamesImported < maxTotalGames)
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

						// Track this game for popularity import
						importedGameIds.Add((long)game.Id);
						gameExternalIdToInternalId[(long)game.Id] = gameEntity.Id;
						totalGamesImported++;
					}
				}

				Console.WriteLine($"Imported {importedGameIds.Count} games (total: {totalGamesImported}/{maxTotalGames}). Now importing their popularity data...");

				// Step 3: Import popularity data for this batch of games
				if (importedGameIds.Any())
				{
					await ImportPopularityForGameBatch(importedGameIds, gameExternalIdToInternalId, popularityTypes);
				}

				offset += limit;
					Console.WriteLine($"Completed batch. Moving to next batch...");
			}

			Console.WriteLine($"Batched game import with popularity sync completed! Total games imported: {totalGamesImported}");
		}

		private async Task ImportPopularityForGameBatch(List<long> gameExternalIds, Dictionary<long, Guid> gameExternalIdToInternalId, int[] popularityTypes)
		{
			var gameIdString = string.Join(",", gameExternalIds);
			var allPopularityData = new List<Popularity>();

			foreach (var popularityType in popularityTypes)
			{
				try
				{
					var popularityResult = await _client.QueryAsync<IGDB.Models.PopularityPrimitive>(
						IGDBClient.Endpoints.PopularityPrimitives,
						$"fields game_id,value,popularity_type; where game_id = ({gameIdString}) & popularity_type = {popularityType};"
					);

					foreach (var p in popularityResult)
					{
						if (gameExternalIdToInternalId.TryGetValue((long)p.GameId, out var internalGameId))
						{
							var popularity = new Popularity
							{
								Id = Guid.NewGuid(),
								ExternalGameId = (long)p.GameId,
								GameId = internalGameId,
								PopularityScore = (decimal)p.Value,
								PopularityType = (long)p.PopularityType.Id,
								CreatedAt = DateTime.UtcNow,
								UpdatedAt = DateTime.UtcNow
							};
							allPopularityData.Add(popularity);
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error fetching popularity data for type {popularityType}: {ex.Message}");
				}
			}

			// Save all popularity data for this batch
			if (allPopularityData.Any())
			{
				try
				{
					await _popularityRepository.InsertOrUpdateAsyncBatch(allPopularityData);
					Console.WriteLine($"Saved {allPopularityData.Count} popularity records for this batch");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error saving popularity data: {ex.Message}");
				}
			}
		}
	}
}
