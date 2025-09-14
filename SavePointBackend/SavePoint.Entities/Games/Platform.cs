using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.Entities.Games
{
	public class Platform : Common.ApiEntity
	{
		public string Name { get; set; }	
		public string Abbreviation { get; set; } = string.Empty;

		//navigation properties
		public ICollection<Game> Games { get; set; } = new List<Game>();
	}
}
