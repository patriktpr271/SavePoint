using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.Entities.Games
{
	public class Company : Common.ApiEntity
	{
        public string Name { get; set; }

        //navigation properties
        public ICollection<GameCompany> GameCompanies { get; set; } = new List<GameCompany>();
	}
}
