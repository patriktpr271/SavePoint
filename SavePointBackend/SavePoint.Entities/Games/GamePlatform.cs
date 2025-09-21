using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.Entities.Games
{
	public class GamePlatform : Common.BaseEntity
	{
		public Guid GameId { get; set; }
		public Game Game { get; set; }
		
		public Guid PlatformId { get; set; }
		public Platform Platform { get; set; }
	}
}
