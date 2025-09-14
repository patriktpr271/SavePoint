using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.Entities.Games
{
	public class Game : Common.ApiEntity
	{
		public string Name { get; set; }
		public string Summary { get; set; } = string.Empty;
		public string CoverUrl { get; set; } = string.Empty;
		public double Rating { get; set; }
		public DateTime ReleaseDate { get; set; }

		//navigation properties
		public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
		public ICollection<GamePlatform> GamePlatforms { get; set; } = new List<GamePlatform>();
		public ICollection<GameCompany> GameCompanies { get; set; } = new List<GameCompany>();
	}
}
