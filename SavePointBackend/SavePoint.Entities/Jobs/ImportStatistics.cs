using SavePoint.Entities.Common;

namespace SavePoint.Entities.Jobs
{
    public class ImportStatistics : BaseEntity
    {
        public required string DataType { get; set; }

        public int TotalRecords { get; set; }
        public DateTime? LastSuccessfulImport { get; set; }

        public DateTime? LastIGDBUpdateDate { get; set; }

        public DateTime? NextScheduledImport { get; set; }

        public bool AutoImportEnabled { get; set; } = true;
        public int ImportFrequencyDays { get; set; } = 3;

        public TimeSpan? AverageImportDuration { get; set; }

        public int SuccessfulImports { get; set; }

        public int FailedImports { get; set; }

        public string? Configuration { get; set; }
    }
}