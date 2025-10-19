using SavePoint.Entities.Common;

namespace SavePoint.Entities.Jobs
{
    /// <summary>
    /// Entity to track background import job runs and their results
    /// </summary>
    public class ImportJobRun : BaseEntity
    {
        public required string JobType { get; set; }

        public ImportJobStatus Status { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsAdded { get; set; }

        public int RecordsUpdated { get; set; }

        public int RecordsFailed { get; set; }
        public string? ErrorMessage { get; set; }

        public string? ErrorDetails { get; set; }

        public string? Metadata { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        public TimeSpan? Duration => CompletedAt?.Subtract(StartedAt);

        public bool IsSuccessful => Status == ImportJobStatus.Completed && string.IsNullOrEmpty(ErrorMessage);
    }
    public enum ImportJobStatus
    {
        Queued = 0,

        Running = 1,

        Completed = 2,

        Failed = 3,

        Cancelled = 4
    }
}