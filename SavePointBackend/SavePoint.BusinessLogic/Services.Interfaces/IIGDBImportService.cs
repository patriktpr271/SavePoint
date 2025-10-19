using SavePoint.Entities.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.BusinessLogic.Services.Interfaces
{
	public interface IIGDBImportService
	{
		/// <summary>
		/// Incremental import for all games with popularity data
		/// Checks existing games by update date, adds new games, and populates popularity
		/// </summary>
		/// <param name="lastUpdateDate">Only import games updated after this date. If null, defaults to 7 days ago.</param>
		Task ImportGamesIncrementalWithPopularityAsync(DateTime? lastUpdateDate = null);
		
		/// <summary>
		/// Incremental import for base data (genres, companies, platforms)
		/// Checks existing data by update date and adds new items
		/// </summary>
		/// <param name="lastUpdateDate">Only import data updated after this date. If null, defaults to 7 days ago.</param>
		Task ImportBaseDataIncrementalAsync(DateTime? lastUpdateDate = null);

		/// <summary>
		/// One-time full database sync - imports all games that don't exist in database
		/// Does not use date filtering, only checks by external game ID
		/// </summary>
		Task ImportAllGamesFullSyncAsync();
	}
}
