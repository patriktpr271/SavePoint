using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.Entities.Games
{
	public class Genre : Common.ApiEntity
	{
		public string Name { get; set; }
		public ICollection<GameGenre> GameGenre { get; set; } = new List<GameGenre>();
	}
}
