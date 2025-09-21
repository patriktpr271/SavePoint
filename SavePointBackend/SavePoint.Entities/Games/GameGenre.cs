using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.Entities.Games
{
	public class GameGenre : Common.BaseEntity
	{
		public Guid GameId { get; set; }
		public Guid GenreId { get; set; }
	}
}
