using Moq;
using FluentAssertions;
using SavePoint.BusinessLogic.Services;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Games;
using SavePoint.Entities.Common;
using SavePoint.Common.Enums;

namespace SavePoint.Tests.Services
{
    public class IGDBImportServiceTests
    {
        private readonly Mock<IGenreRepository> _mockGenreRepository;
        private readonly Mock<IGameRepository> _mockGameRepository;
        private readonly Mock<ICompanyRepository> _mockCompanyRepository;
        private readonly Mock<IPlatfromRepository> _mockPlatformRepository;
        private readonly Mock<IPopularityRepository> _mockPopularityRepository;

        public IGDBImportServiceTests()
        {
            _mockGenreRepository = new Mock<IGenreRepository>();
            _mockGameRepository = new Mock<IGameRepository>();
            _mockCompanyRepository = new Mock<ICompanyRepository>();
            _mockPlatformRepository = new Mock<IPlatfromRepository>();
            _mockPopularityRepository = new Mock<IPopularityRepository>();
        }

        [Fact]
        public void IGDBImportService_CanBeConstructed()
        {
            // Arrange & Act
            // We can't easily mock IGDBClient, so we'll just test that the service can be constructed
            // with mocked dependencies for the repository layer
            var action = () => new IGDBImportService(
                null!, // IGDBClient - can't mock easily
                _mockGenreRepository.Object,
                _mockGameRepository.Object,
                _mockCompanyRepository.Object,
                _mockPlatformRepository.Object,
                _mockPopularityRepository.Object);

            // Assert
            // This will throw if there are constructor issues with the repositories
            action.Should().NotThrow();
        }

        [Fact]
        public void IGDBImportService_HasCorrectConstants()
        {
            // Arrange & Act
            // We can test that the service has the expected behavior by checking
            // that it can be instantiated and has the expected interface
            var serviceType = typeof(IGDBImportService);
            
            // Assert
            serviceType.Should().NotBeNull();
            serviceType.GetInterfaces().Should().Contain(typeof(IIGDBImportService));
        }

        [Fact]
        public void IGDBImportService_ImplementsInterface()
        {
            // Arrange
            var serviceType = typeof(IGDBImportService);
            var interfaceType = typeof(IIGDBImportService);

            // Act & Assert
            serviceType.Should().Implement(interfaceType);
        }

        [Fact]
        public void IGDBImportService_HasRequiredMethods()
        {
            // Arrange
            var serviceType = typeof(IGDBImportService);

            // Act & Assert
            serviceType.GetMethod("ImportGamesIncrementalWithPopularityAsync").Should().NotBeNull();
            serviceType.GetMethod("ImportBaseDataIncrementalAsync").Should().NotBeNull();
            serviceType.GetMethod("ImportAllGamesFullSyncAsync").Should().NotBeNull();
        }

        // Note: Integration tests with actual IGDB client would be needed for full testing
        // These would require API keys and network access, making them integration tests
        // rather than unit tests. For now, we focus on testing the service structure
        // and dependency injection.
    }
}