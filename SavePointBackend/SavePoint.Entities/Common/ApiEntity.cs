using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.Entities.Common
{
	public abstract class ApiEntity
	{
		// EF Core primary key
		public Guid Id { get; set; } = Guid.NewGuid();

		// External ID from IGDB or another API
		public long ExternalId { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
