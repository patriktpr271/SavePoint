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
		Task ImportGenresAsync();
		Task ImportGamesAsync();
		Task ImportCompaniesAsync();
		Task ImportPlatformsAsync();
		Task ImportAllDataAsync();
	}
}
