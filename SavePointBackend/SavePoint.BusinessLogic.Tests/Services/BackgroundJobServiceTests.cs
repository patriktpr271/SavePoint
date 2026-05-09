using Hangfire;
using Microsoft.Extensions.Logging;
using SavePoint.BusinessLogic.Services;
using SavePoint.BusinessLogic.Services.Interfaces;
using SavePoint.DAL.Repositories.Interfaces;
using SavePoint.Entities.Jobs;

namespace SavePoint.BusinessLogic.Tests.Services
{
    public class BackgroundJobServiceTests
    {
        private readonly Mock<IIGDBImportService> _importServiceMock;
        private readonly Mock<IImportJobRunRepository> _jobRunRepoMock;
        private readonly Mock<IImportStatisticsRepository> _statsRepoMock;
        private readonly Mock<ILogger<BackgroundJobService>> _loggerMock;
        private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock;
        private readonly Mock<IRecurringJobManager> _recurringJobManagerMock;
        private readonly BackgroundJobService _sut;

        public BackgroundJobServiceTests()
        {
            _importServiceMock = new Mock<IIGDBImportService>();
            _jobRunRepoMock = new Mock<IImportJobRunRepository>();
            _statsRepoMock = new Mock<IImportStatisticsRepository>();
            _loggerMock = new Mock<ILogger<BackgroundJobService>>();
            _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
            _recurringJobManagerMock = new Mock<IRecurringJobManager>();

            _jobRunRepoMock
                .Setup(r => r.CreateAsync(It.IsAny<ImportJobRun>()))
                .ReturnsAsync((ImportJobRun jr) => jr);
            _jobRunRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<ImportJobRun>()))
                .ReturnsAsync((ImportJobRun jr) => jr);

            _sut = new BackgroundJobService(
                _importServiceMock.Object,
                _jobRunRepoMock.Object,
                _statsRepoMock.Object,
                _loggerMock.Object,
                _backgroundJobClientMock.Object,
                _recurringJobManagerMock.Object);
        }

        // ---------- ExecuteIncrementalGamesWithPopularityImport ----------

        [Fact]
        public async Task ExecuteIncrementalGamesWithPopularityImport_OnSuccess_CompletesJobAndUpdatesStats()
        {
            // Arrange
            _jobRunRepoMock
                .Setup(r => r.GetLastSuccessfulByJobTypeAsync("GamesWithPopularity"))
                .ReturnsAsync((ImportJobRun?)null);

            // Act
            await _sut.ExecuteIncrementalGamesWithPopularityImport();

            // Assert
            _importServiceMock.Verify(s =>
                s.ImportGamesIncrementalWithPopularityAsync(It.IsAny<DateTime?>()), Times.Once);
            _statsRepoMock.Verify(s =>
                s.UpdateImportStatsAsync("GamesWithPopularity", It.IsAny<TimeSpan>(), true), Times.Once);
            _statsRepoMock.Verify(s =>
                s.UpdateLastSuccessfulImportAsync("GamesWithPopularity", It.IsAny<DateTime>()), Times.Once);
            _jobRunRepoMock.Verify(r => r.UpdateAsync(
                It.Is<ImportJobRun>(jr => jr.Status == ImportJobStatus.Completed)), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteIncrementalGamesWithPopularityImport_UsesPriorLastUpdateDate_WhenAvailable()
        {
            // Arrange
            var prior = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
            _jobRunRepoMock
                .Setup(r => r.GetLastSuccessfulByJobTypeAsync("GamesWithPopularity"))
                .ReturnsAsync(new ImportJobRun
                {
                    JobType = "GamesWithPopularity",
                    Status = ImportJobStatus.Completed,
                    LastUpdateDate = prior
                });

            // Act
            await _sut.ExecuteIncrementalGamesWithPopularityImport();

            // Assert
            _importServiceMock.Verify(s => s.ImportGamesIncrementalWithPopularityAsync(prior), Times.Once);
        }

        [Fact]
        public async Task ExecuteIncrementalGamesWithPopularityImport_OnFailure_MarksFailedAndRethrows()
        {
            // Arrange
            _jobRunRepoMock
                .Setup(r => r.GetLastSuccessfulByJobTypeAsync(It.IsAny<string>()))
                .ReturnsAsync((ImportJobRun?)null);
            _importServiceMock
                .Setup(s => s.ImportGamesIncrementalWithPopularityAsync(It.IsAny<DateTime?>()))
                .ThrowsAsync(new InvalidOperationException("upstream IGDB error"));

            // Act
            Func<Task> act = async () => await _sut.ExecuteIncrementalGamesWithPopularityImport();

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            _jobRunRepoMock.Verify(r => r.UpdateAsync(
                It.Is<ImportJobRun>(jr => jr.Status == ImportJobStatus.Failed && jr.ErrorMessage == "upstream IGDB error")),
                Times.AtLeastOnce);
            _statsRepoMock.Verify(s =>
                s.UpdateImportStatsAsync("GamesWithPopularity", It.IsAny<TimeSpan>(), false), Times.Once);
            _statsRepoMock.Verify(s =>
                s.UpdateLastSuccessfulImportAsync(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        // ---------- ExecuteIncrementalBaseDataImport ----------

        [Fact]
        public async Task ExecuteIncrementalBaseDataImport_OnSuccess_CallsImportAndCompletes()
        {
            // Arrange
            _jobRunRepoMock
                .Setup(r => r.GetLastSuccessfulByJobTypeAsync("BaseData"))
                .ReturnsAsync((ImportJobRun?)null);

            // Act
            await _sut.ExecuteIncrementalBaseDataImport();

            // Assert
            _importServiceMock.Verify(s => s.ImportBaseDataIncrementalAsync(It.IsAny<DateTime?>()), Times.Once);
            _statsRepoMock.Verify(s => s.UpdateImportStatsAsync("BaseData", It.IsAny<TimeSpan>(), true), Times.Once);
        }

        // ---------- ExecuteFullDatabaseSync ----------

        [Fact]
        public async Task ExecuteFullDatabaseSync_OnSuccess_CallsImportAndCompletes()
        {
            // Act
            await _sut.ExecuteFullDatabaseSync();

            // Assert
            _importServiceMock.Verify(s => s.ImportAllGamesFullSyncAsync(), Times.Once);
            _statsRepoMock.Verify(s => s.UpdateImportStatsAsync("FullDatabaseSync", It.IsAny<TimeSpan>(), true), Times.Once);
        }

        [Fact]
        public async Task ExecuteFullDatabaseSync_OnFailure_MarksFailedAndRethrows()
        {
            // Arrange
            _importServiceMock
                .Setup(s => s.ImportAllGamesFullSyncAsync())
                .ThrowsAsync(new TimeoutException("IGDB timed out"));

            // Act
            Func<Task> act = async () => await _sut.ExecuteFullDatabaseSync();

            // Assert
            await act.Should().ThrowAsync<TimeoutException>();
            _statsRepoMock.Verify(s => s.UpdateImportStatsAsync("FullDatabaseSync", It.IsAny<TimeSpan>(), false), Times.Once);
        }

        // ---------- Manual wrappers ----------

        [Fact]
        public async Task ManualIncrementalGamesWithPopularityImport_DelegatesToExecuteMethod()
        {
            // Arrange
            _jobRunRepoMock
                .Setup(r => r.GetLastSuccessfulByJobTypeAsync(It.IsAny<string>()))
                .ReturnsAsync((ImportJobRun?)null);

            // Act
            await _sut.ManualIncrementalGamesWithPopularityImport();

            // Assert
            _importServiceMock.Verify(s => s.ImportGamesIncrementalWithPopularityAsync(It.IsAny<DateTime?>()), Times.Once);
        }

        [Fact]
        public async Task ManualIncrementalBaseDataImport_DelegatesToExecuteMethod()
        {
            // Arrange
            _jobRunRepoMock
                .Setup(r => r.GetLastSuccessfulByJobTypeAsync(It.IsAny<string>()))
                .ReturnsAsync((ImportJobRun?)null);

            // Act
            await _sut.ManualIncrementalBaseDataImport();

            // Assert
            _importServiceMock.Verify(s => s.ImportBaseDataIncrementalAsync(It.IsAny<DateTime?>()), Times.Once);
        }

        [Fact]
        public async Task ManualFullDatabaseSync_DelegatesToExecuteMethod()
        {
            // Act
            await _sut.ManualFullDatabaseSync();

            // Assert
            _importServiceMock.Verify(s => s.ImportAllGamesFullSyncAsync(), Times.Once);
        }

        // ---------- GetJobStatusAsync ----------

        [Fact]
        public async Task GetJobStatusAsync_WithMixedRecentJobs_AggregatesCounts()
        {
            // Arrange
            var recentJobs = new List<ImportJobRun>
            {
                new ImportJobRun { JobType = "BaseData", Status = ImportJobStatus.Completed, StartedAt = DateTime.UtcNow.AddDays(-1) },
                new ImportJobRun { JobType = "BaseData", Status = ImportJobStatus.Failed, StartedAt = DateTime.UtcNow.AddDays(-2), ErrorMessage = "boom" },
                new ImportJobRun { JobType = "BaseData", Status = ImportJobStatus.Completed, StartedAt = DateTime.UtcNow.AddDays(-3) }
            };
            _jobRunRepoMock
                .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(recentJobs);
            _jobRunRepoMock
                .Setup(r => r.GetByStatusAsync(ImportJobStatus.Running))
                .ReturnsAsync(new List<ImportJobRun>());
            _jobRunRepoMock
                .Setup(r => r.GetByStatusAsync(ImportJobStatus.Queued))
                .ReturnsAsync(new List<ImportJobRun>());

            // Act
            var status = await _sut.GetJobStatusAsync();

            // Assert
            // Use reflection-friendly assertion: convert to dictionary via FluentAssertions reflection
            status.Should().NotBeNull();
            status.GetType().GetProperty("TotalJobsLastWeek")!.GetValue(status).Should().Be(3);
            status.GetType().GetProperty("SuccessfulJobsLastWeek")!.GetValue(status).Should().Be(2);
            status.GetType().GetProperty("FailedJobsLastWeek")!.GetValue(status).Should().Be(1);
            status.GetType().GetProperty("QueuedJobs")!.GetValue(status).Should().Be(0);
        }

        [Fact]
        public async Task GetJobStatusAsync_WithNoRecentJobs_ReturnsZeroedCounts()
        {
            // Arrange
            _jobRunRepoMock
                .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<ImportJobRun>());
            _jobRunRepoMock
                .Setup(r => r.GetByStatusAsync(It.IsAny<ImportJobStatus>()))
                .ReturnsAsync(new List<ImportJobRun>());

            // Act
            var status = await _sut.GetJobStatusAsync();

            // Assert
            status.GetType().GetProperty("TotalJobsLastWeek")!.GetValue(status).Should().Be(0);
            status.GetType().GetProperty("SuccessfulJobsLastWeek")!.GetValue(status).Should().Be(0);
            status.GetType().GetProperty("FailedJobsLastWeek")!.GetValue(status).Should().Be(0);
        }

        // ---------- GetImportStatisticsAsync ----------

        [Fact]
        public async Task GetImportStatisticsAsync_WithBothSuccessAndFailures_ComputesSuccessRate()
        {
            // Arrange
            var stats = new List<ImportStatistics>
            {
                new ImportStatistics
                {
                    DataType = "GamesWithPopularity",
                    SuccessfulImports = 9,
                    FailedImports = 1,
                    TotalRecords = 100
                }
            };
            _statsRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(stats);

            // Act
            var payload = await _sut.GetImportStatisticsAsync();

            // Assert
            var statsArray = payload.GetType().GetProperty("Statistics")!.GetValue(payload) as System.Collections.IEnumerable;
            statsArray.Should().NotBeNull();
            var first = statsArray!.Cast<object>().First();
            var rate = (double)first.GetType().GetProperty("SuccessRate")!.GetValue(first)!;
            rate.Should().BeApproximately(90.0, 0.001);
        }

        [Fact]
        public async Task GetImportStatisticsAsync_WithNoImportsRecorded_ReturnsZeroSuccessRate()
        {
            // Arrange
            var stats = new List<ImportStatistics>
            {
                new ImportStatistics
                {
                    DataType = "BaseData",
                    SuccessfulImports = 0,
                    FailedImports = 0
                }
            };
            _statsRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(stats);

            // Act
            var payload = await _sut.GetImportStatisticsAsync();

            // Assert
            var statsArray = (System.Collections.IEnumerable)payload.GetType().GetProperty("Statistics")!.GetValue(payload)!;
            var first = statsArray.Cast<object>().First();
            var rate = (double)first.GetType().GetProperty("SuccessRate")!.GetValue(first)!;
            rate.Should().Be(0);
        }
    }
}
