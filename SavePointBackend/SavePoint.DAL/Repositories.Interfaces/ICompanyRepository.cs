using SavePoint.Entities.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePoint.DAL.Repositories.Interfaces
{
	public interface ICompanyRepository
	{
		Task InsertOrUpdateAsyncBatch(List<Company> companies);
		Task<List<Company>> GetByExternalIds(IEnumerable<long> externalIds);
		Task<List<Company>> GetAllAsync();
	}
}
