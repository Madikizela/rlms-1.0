using backend.Models;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace backend.Services
{
    public class DocumentAuditService : IDocumentAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DocumentAuditService> _logger;

        public DocumentAuditService(ApplicationDbContext context, ILogger<DocumentAuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogDocumentAccessAsync(int documentId, int userId, DocumentAccessAction action, 
            string? ipAddress = null, string? userAgent = null, string? additionalInfo = null)
        {
            try
            {
                var auditLog = new DocumentAuditLog
                {
                    DocumentId = documentId,
                    UserId = userId,
                    Action = action.ToString(),
                    Description = $"Document {action.ToString().ToLower()} by user {userId}",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    AdditionalData = additionalInfo,
                    Timestamp = DateTime.UtcNow,
                    IsSecurityEvent = action == DocumentAccessAction.AccessDenied
                };

                _context.DocumentAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Document access logged: DocumentId={DocumentId}, UserId={UserId}, Action={Action}", 
                    documentId, userId, action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log document access: DocumentId={DocumentId}, UserId={UserId}, Action={Action}", 
                    documentId, userId, action);
            }
        }

        public async Task LogDocumentUploadAsync(int documentId, int userId, string fileName, 
            long fileSize, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                var additionalData = JsonSerializer.Serialize(new { fileName, fileSize });
                
                var auditLog = new DocumentAuditLog
                {
                    DocumentId = documentId,
                    UserId = userId,
                    Action = "Upload",
                    Description = $"Document '{fileName}' uploaded by user {userId} (Size: {fileSize} bytes)",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    AdditionalData = additionalData,
                    Timestamp = DateTime.UtcNow,
                    IsSecurityEvent = false
                };

                _context.DocumentAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Document upload logged: DocumentId={DocumentId}, UserId={UserId}, FileName={FileName}", 
                    documentId, userId, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log document upload: DocumentId={DocumentId}, UserId={UserId}, FileName={FileName}", 
                    documentId, userId, fileName);
            }
        }

        public async Task LogDocumentModificationAsync(int documentId, int userId, DocumentModificationType modificationType,
            string? previousValue = null, string? newValue = null, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                var additionalData = JsonSerializer.Serialize(new { 
                    modificationType = modificationType.ToString(), 
                    previousValue, 
                    newValue 
                });

                var auditLog = new DocumentAuditLog
                {
                    DocumentId = documentId,
                    UserId = userId,
                    Action = "Modify",
                    Description = $"Document modified by user {userId}: {modificationType}",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    AdditionalData = additionalData,
                    Timestamp = DateTime.UtcNow,
                    IsSecurityEvent = false
                };

                _context.DocumentAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Document modification logged: DocumentId={DocumentId}, UserId={UserId}, Type={ModificationType}", 
                    documentId, userId, modificationType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log document modification: DocumentId={DocumentId}, UserId={UserId}, Type={ModificationType}", 
                    documentId, userId, modificationType);
            }
        }

        public async Task LogDocumentDeletionAsync(int documentId, int userId, string fileName, 
            string reason, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                var additionalData = JsonSerializer.Serialize(new { fileName, reason });

                var auditLog = new DocumentAuditLog
                {
                    DocumentId = documentId,
                    UserId = userId,
                    Action = "Delete",
                    Description = $"Document '{fileName}' deleted by user {userId}. Reason: {reason}",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    AdditionalData = additionalData,
                    Timestamp = DateTime.UtcNow,
                    IsSecurityEvent = false
                };

                _context.DocumentAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Document deletion logged: DocumentId={DocumentId}, UserId={UserId}, FileName={FileName}", 
                    documentId, userId, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log document deletion: DocumentId={DocumentId}, UserId={UserId}, FileName={FileName}", 
                    documentId, userId, fileName);
            }
        }

        public async Task LogDocumentSharingAsync(int documentId, int userId, int targetUserId, 
            DocumentAccessLevel accessLevel, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                var additionalData = JsonSerializer.Serialize(new { targetUserId, accessLevel = accessLevel.ToString() });

                var auditLog = new DocumentAuditLog
                {
                    DocumentId = documentId,
                    UserId = userId,
                    Action = "Share",
                    Description = $"Document shared by user {userId} with user {targetUserId} (Access: {accessLevel})",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    AdditionalData = additionalData,
                    Timestamp = DateTime.UtcNow,
                    IsSecurityEvent = false
                };

                _context.DocumentAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Document sharing logged: DocumentId={DocumentId}, UserId={UserId}, TargetUserId={TargetUserId}", 
                    documentId, userId, targetUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log document sharing: DocumentId={DocumentId}, UserId={UserId}, TargetUserId={TargetUserId}", 
                    documentId, userId, targetUserId);
            }
        }

        public async Task LogSecurityEventAsync(int? documentId, int? userId, SecurityEventType eventType, 
            string description, string? ipAddress = null, string? userAgent = null, string? additionalData = null)
        {
            try
            {
                var auditLog = new DocumentAuditLog
                {
                    DocumentId = documentId,
                    UserId = userId,
                    Action = "SecurityEvent",
                    Description = description,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    AdditionalData = additionalData,
                    Timestamp = DateTime.UtcNow,
                    IsSecurityEvent = true,
                    SecurityEventType = eventType.ToString()
                };

                _context.DocumentAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Security event logged: EventType={EventType}, Description={Description}, DocumentId={DocumentId}, UserId={UserId}", 
                    eventType, description, documentId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security event: EventType={EventType}, Description={Description}", 
                    eventType, description);
            }
        }

        public async Task<IEnumerable<DocumentAuditLog>> GetDocumentAuditLogsAsync(int documentId, 
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.DocumentAuditLogs
                .Where(log => log.DocumentId == documentId);

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            return await query
                .Include(log => log.User)
                .Include(log => log.Document)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentAuditLog>> GetUserAuditLogsAsync(int userId, 
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.DocumentAuditLogs
                .Where(log => log.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            return await query
                .Include(log => log.User)
                .Include(log => log.Document)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentAuditLog>> GetSecurityEventsAsync(
            DateTime? fromDate = null, DateTime? toDate = null, SecurityEventType? eventType = null)
        {
            var query = _context.DocumentAuditLogs
                .Where(log => log.IsSecurityEvent);

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            if (eventType.HasValue)
                query = query.Where(log => log.SecurityEventType == eventType.ToString());

            return await query
                .Include(log => log.User)
                .Include(log => log.Document)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        public async Task<DocumentAuditReport> GenerateAuditReportAsync(DateTime fromDate, DateTime toDate, 
            int? userId = null, int? documentId = null)
        {
            var query = _context.DocumentAuditLogs
                .Where(log => log.Timestamp >= fromDate && log.Timestamp <= toDate);

            if (userId.HasValue)
                query = query.Where(log => log.UserId == userId.Value);

            if (documentId.HasValue)
                query = query.Where(log => log.DocumentId == documentId.Value);

            var logs = await query
                .Include(log => log.User)
                .Include(log => log.Document)
                .ToListAsync();

            var report = new DocumentAuditReport
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalEvents = logs.Count,
                DocumentAccesses = logs.Count(l => l.Action == "View" || l.Action == "Download" || l.Action == "Print"),
                DocumentUploads = logs.Count(l => l.Action == "Upload"),
                DocumentModifications = logs.Count(l => l.Action == "Modify"),
                DocumentDeletions = logs.Count(l => l.Action == "Delete"),
                SecurityEvents = logs.Count(l => l.IsSecurityEvent),
                UniqueUsers = logs.Where(l => l.UserId.HasValue).Select(l => l.UserId).Distinct().Count(),
                UniqueDocuments = logs.Where(l => l.DocumentId.HasValue).Select(l => l.DocumentId).Distinct().Count(),
                RecentEvents = logs.OrderByDescending(l => l.Timestamp).Take(50).ToList()
            };

            // Security events summary
            report.SecurityEventsSummary = logs
                .Where(l => l.IsSecurityEvent && !string.IsNullOrEmpty(l.SecurityEventType))
                .GroupBy(l => l.SecurityEventType)
                .Select(g => new SecurityEventSummary
                {
                    EventType = Enum.Parse<SecurityEventType>(g.Key!),
                    Count = g.Count(),
                    LastOccurrence = g.Max(l => l.Timestamp)
                })
                .ToList();

            // Top active users
            report.TopActiveUsers = logs
                .Where(l => l.UserId.HasValue && l.User != null)
                .GroupBy(l => new { l.UserId, l.User!.Email })
                .Select(g => new UserActivitySummary
                {
                    UserId = g.Key.UserId!.Value,
                    UserEmail = g.Key.Email,
                    ActivityCount = g.Count(),
                    LastActivity = g.Max(l => l.Timestamp)
                })
                .OrderByDescending(u => u.ActivityCount)
                .Take(10)
                .ToList();

            return report;
        }
    }
}