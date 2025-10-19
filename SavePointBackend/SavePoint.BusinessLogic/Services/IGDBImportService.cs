using IGDB;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Games;
using SavePoint.Common.Enums;
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

		private const int BATCH_SIZE = 250;
		private const int DELAY_BETWEEN_BATCHES_MS = 100;
		private static readonly int[] POPULARITY_TYPES = new int[] { 1, 2, 5 };

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

		/// <summary>
		/// Incremental import for all games with popularity data
		/// Checks existing games by update date, adds new games, and populates popularity
		/// </summary>
		public async Task ImportGamesIncrementalWithPopularityAsync(DateTime? lastUpdateDate = null)
		{
			var updateDate = lastUpdateDate ?? DateTime.UtcNow.AddDays(-7);
			var unixTimestamp = ((DateTimeOffset)updateDate).ToUnixTimeSeconds();

			Console.WriteLine($"Starting incremental games import with popularity for games updated after {updateDate}");

			// Load reference data once
			var genreList = await _genreRepository.GetAllAsync();
			var platformList = await _platformRepository.GetAllAsync();
			var companyList = await _companyRepository.GetAllAsync();

			int offset = 0;
			int totalGamesProcessed = 0;
			int newGamesAdded = 0;
			int existingGamesUpdated = 0;

			while (true)
			{
				Console.WriteLine($"Processing batch starting at offset {offset}...");

				// Get games updated after the specified date
				var games = await _client.QueryAsync<IGDB.Models.Game>(
					 IGDBClient.Endpoints.Games,
					 $"fields id,name,summary,cover.*,first_release_date,genres,platforms,involved_companies.company,involved_companies.developer,involved_companies.publisher,updated_at; " +
					 $"where updated_at >= {unixTimestamp}; " +
					 $"sort updated_at asc; " +
					 $"limit {BATCH_SIZE}; offset {offset};"
				);

				if (games.Count() == 0)
				{
					Console.WriteLine("No more games to process");
					break;
				}

				// Extract external IDs from the batch
				var batchExternalIds = games
					.Where(g => g?.Id.HasValue == true)
					.Select(g => g.Id.Value)
					.ToList();

				// Batch check for existing games - SINGLE DATABASE CALL instead of multiple!
				var existingExternalIds = await _gameRepository.GetExistingExternalIdsAsync(batchExternalIds);

				Console.WriteLine($"Found {existingExternalIds.Count} existing games out of {batchExternalIds.Count} in this batch");

				// Process games and collect their IDs for batch popularity import
				var gameEntities = new List<(Game entity, long externalId)>();
				
				foreach (var game in games)
				{
					if (game != null && game.Id.HasValue)
					{
						try
						{
							// Check if game already exists using the batch result
							bool isNewGame = !existingExternalIds.Contains(game.Id.Value);

							// Create or update game entity
							var gameEntity = await CreateGameEntity(game, genreList, platformList, companyList);
							await _gameRepository.InsertOrUpdateAsync(gameEntity);

							// Store for batch popularity import
							gameEntities.Add((gameEntity, game.Id.Value));

							if (isNewGame)
							{
								newGamesAdded++;
								Console.WriteLine($"Added new game: {game.Name}");
							}
							else
							{
								existingGamesUpdated++;
								Console.WriteLine($"Updated existing game: {game.Name}");
							}

							totalGamesProcessed++;
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error processing game {game.Name}: {ex.Message}");
						}
					}
				}

				// Batch import popularity for all games in this batch
				if (gameEntities.Any())
				{
					await ImportPopularityForGamesBatch(gameEntities);
				}

				Console.WriteLine($"Processed batch of {games.Count()} games. Total: {totalGamesProcessed} (New: {newGamesAdded}, Updated: {existingGamesUpdated})");
				
				offset += BATCH_SIZE;
				await Task.Delay(DELAY_BETWEEN_BATCHES_MS);
			}

			Console.WriteLine($"Incremental games import completed! Total processed: {totalGamesProcessed}, New: {newGamesAdded}, Updated: {existingGamesUpdated}");
		}

		/// <summary>
		/// Incremental import for base data (genres, companies, platforms)
		/// Checks existing data by update date and adds new items
		/// </summary>
		public async Task ImportBaseDataIncrementalAsync(DateTime? lastUpdateDate = null)
		{
			var updateDate = lastUpdateDate ?? DateTime.UtcNow.AddDays(-7);
			var unixTimestamp = ((DateTimeOffset)updateDate).ToUnixTimeSeconds();

			Console.WriteLine($"Starting incremental base data import for data updated after {updateDate}");

			// Import genres incrementally
			await ImportGenresIncrementalAsync(unixTimestamp);
			
			// Import companies incrementally
			await ImportCompaniesIncrementalAsync(unixTimestamp);
			
			// Import platforms incrementally
			await ImportPlatformsIncrementalAsync(unixTimestamp);

			Console.WriteLine("Incremental base data import completed");
		}

		/// <summary>
		/// One-time full database sync - imports all games that don't exist in database
		/// Does not use date filtering, only checks by external game ID
		/// </summary>
		public async Task ImportAllGamesFullSyncAsync()
		{
			Console.WriteLine("Starting full database sync - importing all games not in database");

			// Load reference data once
			var genreList = await _genreRepository.GetAllAsync();
			var platformList = await _platformRepository.GetAllAsync();
			var companyList = await _companyRepository.GetAllAsync();

			// Get all existing external game IDs from database once at the start
			var existingGames = await _gameRepository.GetAllAsync();
			var existingExternalIds = existingGames
				.Where(g => g.ExternalId.HasValue)
				.Select(g => g.ExternalId.Value)
				.ToHashSet();

			Console.WriteLine($"Found {existingExternalIds.Count} existing games in database");

			int offset = 0;
			int totalGamesProcessed = 0;
			int newGamesAdded = 0;
			int skippedExistingGames = 0;

			while (true)
			{
				Console.WriteLine($"Processing batch starting at offset {offset}...");

				// Get all games (no date filter)
				var games = await _client.QueryAsync<IGDB.Models.Game>(
					 IGDBClient.Endpoints.Games,
					 $"fields id,name,summary,cover.*,first_release_date,genres,platforms,involved_companies.company,involved_companies.developer,involved_companies.publisher; " +
					 $"sort id asc; " +
					 $"limit {BATCH_SIZE}; offset {offset};"
				);

				if (games.Count() == 0)
				{
					Console.WriteLine("No more games to process");
					break;
				}

				// Extract external IDs from the batch for additional verification
				var batchExternalIds = games
					.Where(g => g?.Id.HasValue == true)
					.Select(g => g.Id.Value)
					.ToList();

				// Batch check for any new games that might have been added since start
				// This is optional but helps with concurrent imports
				var currentExistingIds = await _gameRepository.GetExistingExternalIdsAsync(batchExternalIds);
				
				// Merge with our initial set
				foreach (var id in currentExistingIds)
				{
					existingExternalIds.Add(id);
				}

				// Process games and collect new ones for batch popularity import
				var newGameEntities = new List<(Game entity, long externalId)>();
				
				foreach (var game in games)
				{
					if (game != null && game.Id.HasValue)
					{
						try
						{
							// Check if game already exists by external ID
							if (existingExternalIds.Contains(game.Id.Value))
							{
								skippedExistingGames++;
								continue; // Skip if already exists
							}

							// Create new game entity
							var gameEntity = await CreateGameEntity(game, genreList, platformList, companyList);
							await _gameRepository.InsertOrUpdateAsync(gameEntity);

							// Store for batch popularity import
							newGameEntities.Add((gameEntity, game.Id.Value));

							// Add to existing set to avoid duplicates in same run
							existingExternalIds.Add(game.Id.Value);

							newGamesAdded++;
							Console.WriteLine($"Added new game: {game.Name}");
							totalGamesProcessed++;
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error processing game {game.Name}: {ex.Message}");
						}
					}
				}

				// Batch import popularity for all new games in this batch
				if (newGameEntities.Any())
				{
					await ImportPopularityForGamesBatch(newGameEntities);
				}

				Console.WriteLine($"Processed batch of {games.Count()} games. New: {newGamesAdded}, Skipped: {skippedExistingGames}, Total processed: {totalGamesProcessed}");
				
				offset += BATCH_SIZE;
				await Task.Delay(DELAY_BETWEEN_BATCHES_MS);
			}

			Console.WriteLine($"Full database sync completed! New games added: {newGamesAdded}, Total processed: {totalGamesProcessed}, Skipped existing: {skippedExistingGames}");
		}

		// Helper methods

		private async Task ImportGenresIncrementalAsync(long unixTimestamp)
		{
			Console.WriteLine("Starting incremental genres import...");

			int offset = 0;
			int totalProcessed = 0;
			bool hasMore = true;

			while (hasMore)
			{
				var response = await _client.QueryAsync<IGDB.Models.Genre>(
					IGDBClient.Endpoints.Genres,
					$"fields id,name,updated_at; where updated_at >= {unixTimestamp}; limit {BATCH_SIZE}; offset {offset};"
				);

				if (response.Count() == 0)
				{
					hasMore = false;
					break;
				}

				foreach (var g in response)
				{
					try
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
						totalProcessed++;
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error processing genre {g.Name}: {ex.Message}");
					}
				}

				Console.WriteLine($"Processed {response.Count()} genres");
				offset += BATCH_SIZE;
				await Task.Delay(DELAY_BETWEEN_BATCHES_MS);
			}

			Console.WriteLine($"Incremental genres import completed. Total processed: {totalProcessed}");
		}

		private async Task ImportCompaniesIncrementalAsync(long unixTimestamp)
		{
			Console.WriteLine("Starting incremental companies import...");

			int offset = 0;
			bool hasMore = true;
			var companyBatch = new List<Company>();

			while (hasMore)
			{
				var result = await _client.QueryAsync<IGDB.Models.Company>(
					IGDBClient.Endpoints.Companies,
					$"fields id,name,description,logo.*,updated_at; where updated_at >= {unixTimestamp}; limit {BATCH_SIZE}; offset {offset};"
				);

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
					companyBatch.Add(company);
				}

				if (companyBatch.Any())
				{
					await _companyRepository.InsertOrUpdateAsyncBatch(companyBatch);
					Console.WriteLine($"Processed {companyBatch.Count} companies");
					companyBatch.Clear();
				}

				offset += BATCH_SIZE;
				await Task.Delay(DELAY_BETWEEN_BATCHES_MS);
			}

			Console.WriteLine("Incremental companies import completed");
		}

		private async Task ImportPlatformsIncrementalAsync(long unixTimestamp)
		{
			Console.WriteLine("Starting incremental platforms import...");

			int offset = 0;
			bool hasMore = true;
			var platformBatch = new List<Platform>();

			while (hasMore)
			{
				var result = await _client.QueryAsync<IGDB.Models.Platform>(
					IGDBClient.Endpoints.Platforms,
					$"fields id,name,abbreviation,updated_at; where updated_at >= {unixTimestamp}; limit {BATCH_SIZE}; offset {offset};"
				);

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
					platformBatch.Add(platform);
				}

				if (platformBatch.Any())
				{
					await _platformRepository.InsertOrUpdateAsyncBatch(platformBatch);
					Console.WriteLine($"Processed {platformBatch.Count} platforms");
					platformBatch.Clear();
				}

				offset += BATCH_SIZE;
				await Task.Delay(DELAY_BETWEEN_BATCHES_MS);
			}

			Console.WriteLine("Incremental platforms import completed");
		}

		private async Task ImportPopularityForGamesBatch(List<(Game entity, long externalId)> gameEntities)
		{
			if (!gameEntities.Any()) return;

			try
			{
				var gameExternalIds = gameEntities.Select(g => g.externalId).ToList();
				var gameIdString = string.Join(",", gameExternalIds);
				var gameIdMap = gameEntities.ToDictionary(g => g.externalId, g => g.entity.Id);

				Console.WriteLine($"Importing popularity for {gameEntities.Count} games...");

				var allPopularityData = new List<Popularity>();

				// Fetch popularity for all games in batch for each popularity type
				foreach (var popularityType in POPULARITY_TYPES)
				{
					try
					{
						var popularityResult = await _client.QueryAsync<IGDB.Models.PopularityPrimitive>(
							IGDBClient.Endpoints.PopularityPrimitives,
							$"fields game_id,value,popularity_type; where game_id = ({gameIdString}) & popularity_type = {popularityType};"
						);

						foreach (var p in popularityResult)
						{
							if (p.GameId.HasValue && gameIdMap.TryGetValue(p.GameId.Value, out var internalGameId))
							{
								var popularity = new Popularity
								{
									Id = Guid.NewGuid(),
									ExternalGameId = p.GameId.Value,
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
						Console.WriteLine($"Error fetching popularity type {popularityType}: {ex.Message}");
					}
				}

				// Save all popularity data in one batch
				if (allPopularityData.Any())
				{
					await _popularityRepository.InsertOrUpdateAsyncBatch(allPopularityData);
					Console.WriteLine($"Imported {allPopularityData.Count} popularity records");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in batch popularity import: {ex.Message}");
			}
		}

		private async Task ImportPopularityForGame(long externalGameId, Guid internalGameId)
		{
			// Keep this method for backward compatibility, but use batch method when possible
			await ImportPopularityForGamesBatch(new List<(Game, long)> { (new Game { Id = internalGameId }, externalGameId) });
		}

		private async Task<Game> CreateGameEntity(IGDB.Models.Game game, 
			IEnumerable<Genre> genreList, 
			IEnumerable<Platform> platformList, 
			IEnumerable<Company> companyList)
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

			return gameEntity;
		}
	}
}
