using AutoMapper;
using SavePoint.BusinessLogic.Services;
using SavePoint.Common.Dtos.Lookups;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Games;

namespace SavePoint.BusinessLogic.Tests.Services
{
    public class LookupServiceTests
    {
        private readonly Mock<IGenreRepository> _genreRepoMock;
        private readonly Mock<IPlatfromRepository> _platformRepoMock;
        private readonly Mock<ICompanyRepository> _companyRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly LookupService _sut;

        public LookupServiceTests()
        {
            _genreRepoMock = new Mock<IGenreRepository>();
            _platformRepoMock = new Mock<IPlatfromRepository>();
            _companyRepoMock = new Mock<ICompanyRepository>();
            _mapperMock = new Mock<IMapper>();
            _sut = new LookupService(
                _genreRepoMock.Object,
                _platformRepoMock.Object,
                _companyRepoMock.Object,
                _mapperMock.Object);
        }

        // ---------- GetGenresAsync ----------

        [Fact]
        public async Task GetGenresAsync_WithUnsortedGenres_PassesNameSortedListToMapper()
        {
            // Arrange
            var genres = new List<Genre>
            {
                new Genre { Id = Guid.NewGuid(), Name = "RPG" },
                new Genre { Id = Guid.NewGuid(), Name = "Action" },
                new Genre { Id = Guid.NewGuid(), Name = "Strategy" }
            };
            IOrderedEnumerable<Genre>? capturedOrder = null;
            _genreRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(genres);
            _mapperMock
                .Setup(m => m.Map<List<GenreDto>>(It.IsAny<IOrderedEnumerable<Genre>>()))
                .Callback((object src) => capturedOrder = (IOrderedEnumerable<Genre>)src)
                .Returns((object src) => ((IEnumerable<Genre>)src).Select(g => new GenreDto { Id = g.Id, Name = g.Name }).ToList());

            // Act
            var result = await _sut.GetGenresAsync();

            // Assert
            capturedOrder.Should().NotBeNull();
            capturedOrder!.Select(g => g.Name).Should().ContainInOrder("Action", "RPG", "Strategy");
            result.Select(d => d.Name).Should().ContainInOrder("Action", "RPG", "Strategy");
        }

        [Fact]
        public async Task GetGenresAsync_WithEmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            _genreRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Genre>());
            _mapperMock
                .Setup(m => m.Map<List<GenreDto>>(It.IsAny<IOrderedEnumerable<Genre>>()))
                .Returns(new List<GenreDto>());

            // Act
            var result = await _sut.GetGenresAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetGenresAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _genreRepoMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new InvalidOperationException("boom"));

            // Act
            Func<Task> act = async () => await _sut.GetGenresAsync();

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        // ---------- GetPlatformsAsync ----------

        [Fact]
        public async Task GetPlatformsAsync_WithUnsortedPlatforms_PassesNameSortedListToMapper()
        {
            // Arrange
            var platforms = new List<Platform>
            {
                new Platform { Id = Guid.NewGuid(), Name = "Xbox" },
                new Platform { Id = Guid.NewGuid(), Name = "PC" },
                new Platform { Id = Guid.NewGuid(), Name = "PlayStation" }
            };
            IOrderedEnumerable<Platform>? capturedOrder = null;
            _platformRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(platforms);
            _mapperMock
                .Setup(m => m.Map<List<PlatformDto>>(It.IsAny<IOrderedEnumerable<Platform>>()))
                .Callback((object src) => capturedOrder = (IOrderedEnumerable<Platform>)src)
                .Returns((object src) => ((IEnumerable<Platform>)src).Select(p => new PlatformDto { Id = p.Id, Name = p.Name }).ToList());

            // Act
            var result = await _sut.GetPlatformsAsync();

            // Assert
            capturedOrder!.Select(p => p.Name).Should().ContainInOrder("PC", "PlayStation", "Xbox");
            result.Select(d => d.Name).Should().ContainInOrder("PC", "PlayStation", "Xbox");
        }

        [Fact]
        public async Task GetPlatformsAsync_WithEmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            _platformRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Platform>());
            _mapperMock
                .Setup(m => m.Map<List<PlatformDto>>(It.IsAny<IOrderedEnumerable<Platform>>()))
                .Returns(new List<PlatformDto>());

            // Act
            var result = await _sut.GetPlatformsAsync();

            // Assert
            result.Should().BeEmpty();
        }

        // ---------- GetCompaniesAsync ----------

        [Fact]
        public async Task GetCompaniesAsync_WithUnsortedCompanies_PassesNameSortedListToMapper()
        {
            // Arrange
            var companies = new List<Company>
            {
                new Company { Id = Guid.NewGuid(), Name = "Ubisoft" },
                new Company { Id = Guid.NewGuid(), Name = "Bethesda" },
                new Company { Id = Guid.NewGuid(), Name = "CD Projekt" }
            };
            IOrderedEnumerable<Company>? capturedOrder = null;
            _companyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(companies);
            _mapperMock
                .Setup(m => m.Map<List<CompanyDto>>(It.IsAny<IOrderedEnumerable<Company>>()))
                .Callback((object src) => capturedOrder = (IOrderedEnumerable<Company>)src)
                .Returns((object src) => ((IEnumerable<Company>)src).Select(c => new CompanyDto { Id = c.Id, Name = c.Name }).ToList());

            // Act
            var result = await _sut.GetCompaniesAsync();

            // Assert
            capturedOrder!.Select(c => c.Name).Should().ContainInOrder("Bethesda", "CD Projekt", "Ubisoft");
            result.Select(d => d.Name).Should().ContainInOrder("Bethesda", "CD Projekt", "Ubisoft");
        }

        [Fact]
        public async Task GetCompaniesAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _companyRepoMock.Setup(r => r.GetAllAsync()).ThrowsAsync(new TimeoutException("timeout"));

            // Act
            Func<Task> act = async () => await _sut.GetCompaniesAsync();

            // Assert
            await act.Should().ThrowAsync<TimeoutException>();
        }
    }
}
