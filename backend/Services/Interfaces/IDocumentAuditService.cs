using backend.Models;

namespace backend.Services.Interfaces
{
    public interface IDocumentAuditService
    {
        /// <summary>
        /// Logs document access events
        /// </summary>
        Task LogDocumentAccessAsync(int documentId, int userId, DocumentAccessAction action, 
            string? ipAddress = null, string? userAgent = null, string? additionalInfo = null);

        /// <summary>
        /// Logs document upload events
        /// </summary>
        Task LogDocumentUploadAsync(int documentId, int userId, string fileName, 
            long fileSize, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Logs document modification events
        /// </summary>
        Task LogDocumentModificationAsync(int documentId, int userId, DocumentModificationType modificationType,
            string? previousValue = null, string? newValue = null, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Logs document deletion events
        /// </summary>
        Task LogDocumentDeletionAsync(int documentId, int userId, string fileName, 
            string reason, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Logs document sharing events
        /// </summary>
        Task LogDocumentSharingAsync(int documentId, int userId, int targetUserId, 
            DocumentAccessLevel accessLevel, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Logs security events (failed access attempts, suspicious activities)
        /// </summary>
        Task LogSecurityEventAsync(int? documentId, int? userId, SecurityEventType eventType, 
            string description, string? ipAddress = null, string? userAgent = null, 
            string? additionalData = null);

        /// <summary>
        /// Retrieves audit logs for a specific document
        /// </summary>
        Task<IEnumerable<DocumentAuditLog>> GetDocumentAuditLogsAsync(int documentId, 
            DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Retrieves audit logs for a specific user
        /// </summary>
        Task<IEnumerable<DocumentAuditLog>> GetUserAuditLogsAsync(int userId, 
            DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Retrieves security events
        /// </summary>
        Task<IEnumerable<DocumentAuditLog>> GetSecurityEventsAsync(
            DateTime? fromDate = null, DateTime? toDate = null, SecurityEventType? eventType = null);

        /// <summary>
        /// Generates audit report for compliance
        /// </summary>
        Task<DocumentAuditReport> GenerateAuditReportAsync(DateTime fromDate, DateTime toDate, 
            int? userId = null, int? documentId = null);
    }

    public enum DocumentAccessAction
    {
        View = 1,
        Download = 2,
        Upload = 3,
        Update = 4,
        Delete = 5,
        Share = 6,
        Approve = 7,
        Reject = 8,
        Print = 9,
        AccessDenied = 10
    }

    public enum DocumentModificationType
    {
        MetadataUpdate = 1,
        AccessLevelChange = 2,
        StatusChange = 3,
        OwnershipTransfer = 4,
        EncryptionUpdate = 5
    }

    public enum SecurityEventType
    {
        UnauthorizedAccess = 1,
        SuspiciousActivity = 2,
        MalwareDetected = 3,
        IntegrityViolation = 4,
        EncryptionFailure = 5,
        ExcessiveFailedAttempts = 6,
        DataBreach = 7
    }

    public class DocumentAuditLog
    {
        public int Id { get; set; }
        public int? DocumentId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? AdditionalData { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsSecurityEvent { get; set; }
        public string? SecurityEventType { get; set; }

        // Navigation properties
        public Document? Document { get; set; }
        public User? User { get; set; }
    }

    public class DocumentAuditReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalEvents { get; set; }
        public int DocumentAccesses { get; set; }
        public int DocumentUploads { get; set; }
        public int DocumentModifications { get; set; }
        public int DocumentDeletions { get; set; }
        public int SecurityEvents { get; set; }
        public int UniqueUsers { get; set; }
        public int UniqueDocuments { get; set; }
        public List<DocumentAuditLog> RecentEvents { get; set; } = new();
        public List<SecurityEventSummary> SecurityEventsSummary { get; set; } = new();
        public List<UserActivitySummary> TopActiveUsers { get; set; } = new();
    }

    public class SecurityEventSummary
    {
        public SecurityEventType EventType { get; set; }
        public int Count { get; set; }
        public DateTime LastOccurrence { get; set; }
    }

    public class UserActivitySummary
    {
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public int ActivityCount { get; set; }
        public DateTime LastActivity { get; set; }
    }
}