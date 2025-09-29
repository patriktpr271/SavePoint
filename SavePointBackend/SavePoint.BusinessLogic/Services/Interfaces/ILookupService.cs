using SavePoint.Common.Dtos.Lookups;

namespace SavePoint.BusinessLogic.Services.Interfaces
{
    public interface ILookupService
    {
        Task<List<GenreDto>> GetGenresAsync();
        Task<List<PlatformDto>> GetPlatformsAsync();
        Task<List<CompanyDto>> GetCompaniesAsync();
    }
}