using AutoMapper;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.Common.Dtos.Lookups;
using SavePoint.DAL.Repositories.Interfaces;

namespace SavePoint.BusinessLogic.Services
{
    public class LookupService : ILookupService
    {
        private readonly IGenreRepository _genreRepository;
        private readonly IPlatfromRepository _platformRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;

        public LookupService(
            IGenreRepository genreRepository,
            IPlatfromRepository platformRepository,
            ICompanyRepository companyRepository,
            IMapper mapper)
        {
            _genreRepository = genreRepository;
            _platformRepository = platformRepository;
            _companyRepository = companyRepository;
            _mapper = mapper;
        }

        public async Task<List<GenreDto>> GetGenresAsync()
        {
            var genres = await _genreRepository.GetAllAsync();
            return _mapper.Map<List<GenreDto>>(genres.OrderBy(g => g.Name));
        }

        public async Task<List<PlatformDto>> GetPlatformsAsync()
        {
            var platforms = await _platformRepository.GetAllAsync();
            return _mapper.Map<List<PlatformDto>>(platforms.OrderBy(p => p.Name));
        }

        public async Task<List<CompanyDto>> GetCompaniesAsync()
        {
            var companies = await _companyRepository.GetAllAsync();
            return _mapper.Map<List<CompanyDto>>(companies.OrderBy(c => c.Name));
        }
    }
}