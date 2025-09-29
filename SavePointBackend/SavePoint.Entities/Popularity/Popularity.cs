using SavePoint.Entities.Common;
using SavePoint.Entities.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.Entities.Popularity
{
	public class Popularity : ApiEntity
	{
		public long ExternalGameId { get; set; }
		public Guid GameId { get; set; }
		public decimal PopularityScore { get; set; }
		public long PopularityType { get; set; }
		public Game Game { get; set; }
	}
}
