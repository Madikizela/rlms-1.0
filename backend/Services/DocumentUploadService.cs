using backend.Models;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace backend.Services
{
    /// <summary>
    /// Secure document upload service with comprehensive validation and security features
    /// </summary>
    public class DocumentUploadService : IDocumentUploadService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileEncryptionService _encryptionService;
        private readonly ILogger<DocumentUploadService> _logger;
        private readonly IConfiguration _configuration;

        // File type configurations
        private readonly Dictionary<DocumentType, List<string>> _allowedFileTypes;
        private readonly Dictionary<DocumentType, long> _maxFileSizes;

        public DocumentUploadService(
            ApplicationDbContext context,
            IFileEncryptionService encryptionService,
            ILogger<DocumentUploadService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _encryptionService = encryptionService;
            _logger = logger;
            _configuration = configuration;

            // Initialize file type restrictions
            _allowedFileTypes = InitializeAllowedFileTypes();
            _maxFileSizes = InitializeMaxFileSizes();
        }

        /// <summary>
        /// Uploads a document with security validation and encryption
        /// </summary>
        public async Task<DocumentUploadResult> UploadDocumentAsync(IFormFile file, DocumentUploadRequest uploadRequest, int userId)
        {
            var result = new DocumentUploadResult();

            try
            {
                _logger.LogInformation("Starting document upload for user {UserId}, file: {FileName}", userId, file.FileName);

                // Validate file
                var validationResult = await ValidateFileAsync(file, uploadRequest.Type);
                result.ValidationResult = validationResult;

                if (!validationResult.IsValid)
                {
                    result.Success = false;
                    result.Message = "File validation failed";
                    result.Errors = validationResult.Errors;
                    return result;
                }

                // Create secure storage directory
                var storageDirectory = CreateSecureStorageDirectory(uploadRequest.Type, userId);
                Directory.CreateDirectory(storageDirectory);

                // Generate secure file name
                var secureFileName = GenerateSecureFileName(file.FileName, uploadRequest.Type);
                var tempFilePath = Path.Combine(storageDirectory, secureFileName);

                // Save file temporarily
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Calculate file hash before encryption
                var originalHash = await _encryptionService.CalculateFileHashAsync(tempFilePath);

                // Scan for malware (placeholder)
                var scanResult = await ScanFileForMalwareAsync(tempFilePath);
                result.ScanResult = scanResult;

                if (!scanResult.IsClean)
                {
                    // Delete the file and return error
                    await _encryptionService.SecureDeleteFileAsync(tempFilePath);
                    result.Success = false;
                    result.Message = "File failed security scan";
                    result.Errors.Add("Potential malware detected");
                    return result;
                }

                // Encrypt the file
                var encryptionResult = await _encryptionService.EncryptFileAsync(tempFilePath);

                // Delete original temporary file
                await _encryptionService.SecureDeleteFileAsync(tempFilePath);

                // Create document entity
                var document = new Document
                {
                    Name = uploadRequest.Name,
                    OriginalFileName = file.FileName,
                    FilePath = encryptionResult.EncryptedFilePath ?? "",
                    ContentType = file.ContentType,
                    FileSizeBytes = file.Length,
                    Type = uploadRequest.Type,
                    Status = uploadRequest.RequiresApproval ? DocumentStatus.PendingApproval : DocumentStatus.Active,
                    Description = uploadRequest.Description,
                    FileHash = originalHash,
                    IsEncrypted = true,
                    EncryptionAlgorithm = encryptionResult.Algorithm,
                    EncryptionKey = encryptionResult.EncryptionKey,
                    AccessLevel = uploadRequest.AccessLevel,
                    RequiresApproval = uploadRequest.RequiresApproval,
                    ExpiresAt = uploadRequest.ExpiresAt,
                    UploadedByUserId = userId,
                    ClientId = uploadRequest.ClientId,
                    SkillsDevelopmentProviderId = uploadRequest.SkillsDevelopmentProviderId,
                    DepartmentId = uploadRequest.DepartmentId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Save to database
                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                // Log access
                await LogDocumentAccessAsync(document.Id, userId, DocumentAccessAction.Upload, "Document uploaded successfully");

                result.Success = true;
                result.Message = "Document uploaded successfully";
                result.Document = document;

                _logger.LogInformation("Document uploaded successfully: {DocumentId} for user {UserId}", document.Id, userId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for user {UserId}", userId);
                result.Success = false;
                result.Message = "An error occurred during upload";
                result.Errors.Add("Internal server error");
                return result;
            }
        }

        /// <summary>
        /// Validates file before upload
        /// </summary>
        public async Task<FileValidationResult> ValidateFileAsync(IFormFile file, DocumentType documentType)
        {
            var result = new FileValidationResult
            {
                FileSize = file.Length,
                FileExtension = Path.GetExtension(file.FileName).ToLowerInvariant(),
                DetectedMimeType = file.ContentType
            };

            try
            {
                // Check if file is empty
                if (file.Length == 0)
                {
                    result.Errors.Add("File is empty");
                }

                // Check file size
                var maxSize = GetMaxFileSize(documentType);
                if (file.Length > maxSize)
                {
                    result.Errors.Add($"File size ({file.Length:N0} bytes) exceeds maximum allowed size ({maxSize:N0} bytes)");
                }

                // Check file extension
                var allowedTypes = GetAllowedFileTypes(documentType);
                if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
                {
                    result.Errors.Add($"File type '{file.ContentType}' is not allowed for {documentType} documents");
                }

                // Validate file name
                if (string.IsNullOrWhiteSpace(file.FileName))
                {
                    result.Errors.Add("File name is required");
                }
                else if (ContainsDangerousCharacters(file.FileName))
                {
                    result.Errors.Add("File name contains invalid characters");
                }

                // Check for double extensions (potential security risk)
                if (HasDoubleExtension(file.FileName))
                {
                    result.Warnings.Add("File has multiple extensions, which may indicate a security risk");
                }

                // Additional MIME type validation by reading file header
                var detectedMimeType = await DetectMimeTypeFromContent(file);
                if (!string.IsNullOrEmpty(detectedMimeType) && detectedMimeType != file.ContentType)
                {
                    result.Warnings.Add($"Declared MIME type ({file.ContentType}) differs from detected type ({detectedMimeType})");
                }

                result.IsValid = result.Errors.Count == 0;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file: {FileName}", file.FileName);
                result.Errors.Add("File validation failed");
                result.IsValid = false;
                return result;
            }
        }

        /// <summary>
        /// Gets allowed file types for a specific document type
        /// </summary>
        public List<string> GetAllowedFileTypes(DocumentType documentType)
        {
            return _allowedFileTypes.GetValueOrDefault(documentType, new List<string>());
        }

        /// <summary>
        /// Gets maximum file size for a specific document type
        /// </summary>
        public long GetMaxFileSize(DocumentType documentType)
        {
            return _maxFileSizes.GetValueOrDefault(documentType, 10 * 1024 * 1024); // Default 10MB
        }

        /// <summary>
        /// Scans file for malware (placeholder implementation)
        /// </summary>
        public async Task<MalwareScanResult> ScanFileForMalwareAsync(string filePath)
        {
            // Placeholder implementation - in production, integrate with actual antivirus service
            await Task.Delay(100); // Simulate scan time

            return new MalwareScanResult
            {
                IsClean = true,
                ScanEngine = "PlaceholderScanner",
                ScanDate = DateTime.UtcNow,
                ScanId = Guid.NewGuid().ToString()
            };
        }

        /// <summary>
        /// Generates secure file name to prevent path traversal attacks
        /// </summary>
        public string GenerateSecureFileName(string originalFileName, DocumentType documentType)
        {
            // Remove path information
            var fileName = Path.GetFileName(originalFileName);
            
            // Remove dangerous characters
            var sanitized = Regex.Replace(fileName, @"[^\w\-_\.]", "_");
            
            // Ensure it doesn't start with a dot
            if (sanitized.StartsWith("."))
            {
                sanitized = "file" + sanitized;
            }

            // Add timestamp and random component for uniqueness
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var random = Guid.NewGuid().ToString("N")[..8];
            var extension = Path.GetExtension(sanitized);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitized);

            return $"{documentType}_{timestamp}_{random}_{nameWithoutExtension}{extension}";
        }

        /// <summary>
        /// Creates secure storage directory structure
        /// </summary>
        public string CreateSecureStorageDirectory(DocumentType documentType, int userId)
        {
            var baseStoragePath = _configuration.GetValue<string>("DocumentStorage:BasePath") ?? "uploads";
            var year = DateTime.UtcNow.Year.ToString();
            var month = DateTime.UtcNow.Month.ToString("D2");
            
            return Path.Combine(baseStoragePath, documentType.ToString(), year, month, userId.ToString());
        }

        /// <summary>
        /// Initializes allowed file types for each document type
        /// </summary>
        private Dictionary<DocumentType, List<string>> InitializeAllowedFileTypes()
        {
            return new Dictionary<DocumentType, List<string>>
            {
                [DocumentType.Identity] = new List<string>
                {
                    "application/pdf", "image/jpeg", "image/png", "image/tiff"
                },
                [DocumentType.PortfolioOfEvidence] = new List<string>
                {
                    "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "image/jpeg", "image/png", "video/mp4", "video/avi"
                },
                [DocumentType.Contract] = new List<string>
                {
                    "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                },
                [DocumentType.Invoice] = new List<string>
                {
                    "application/pdf", "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                },
                [DocumentType.Certificate] = new List<string>
                {
                    "application/pdf", "image/jpeg", "image/png"
                },
                [DocumentType.Assessment] = new List<string>
                {
                    "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                },
                [DocumentType.Report] = new List<string>
                {
                    "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                },
                [DocumentType.Policy] = new List<string>
                {
                    "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                },
                [DocumentType.Other] = new List<string>
                {
                    "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "image/jpeg", "image/png", "text/plain"
                }
            };
        }

        /// <summary>
        /// Initializes maximum file sizes for each document type
        /// </summary>
        private Dictionary<DocumentType, long> InitializeMaxFileSizes()
        {
            return new Dictionary<DocumentType, long>
            {
                [DocumentType.Identity] = 5 * 1024 * 1024,           // 5MB
                [DocumentType.PortfolioOfEvidence] = 50 * 1024 * 1024, // 50MB
                [DocumentType.Contract] = 10 * 1024 * 1024,          // 10MB
                [DocumentType.Invoice] = 5 * 1024 * 1024,            // 5MB
                [DocumentType.Certificate] = 5 * 1024 * 1024,        // 5MB
                [DocumentType.Assessment] = 20 * 1024 * 1024,        // 20MB
                [DocumentType.Report] = 25 * 1024 * 1024,            // 25MB
                [DocumentType.Policy] = 10 * 1024 * 1024,            // 10MB
                [DocumentType.Other] = 10 * 1024 * 1024              // 10MB
            };
        }

        /// <summary>
        /// Checks if filename contains dangerous characters
        /// </summary>
        private bool ContainsDangerousCharacters(string fileName)
        {
            var dangerousChars = new[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|', '\0' };
            return fileName.IndexOfAny(dangerousChars) >= 0 || fileName.Contains("..");
        }

        /// <summary>
        /// Checks if file has double extension (potential security risk)
        /// </summary>
        private bool HasDoubleExtension(string fileName)
        {
            var parts = fileName.Split('.');
            return parts.Length > 2;
        }

        /// <summary>
        /// Detects MIME type from file content
        /// </summary>
        private async Task<string> DetectMimeTypeFromContent(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var buffer = new byte[512];
                await stream.ReadAsync(buffer, 0, buffer.Length);

                // Simple file signature detection
                if (buffer.Length >= 4)
                {
                    // PDF signature
                    if (buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46)
                        return "application/pdf";

                    // JPEG signature
                    if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                        return "image/jpeg";

                    // PNG signature
                    if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47)
                        return "image/png";
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Logs document access for audit trail
        /// </summary>
        private async Task LogDocumentAccessAsync(int documentId, int userId, DocumentAccessAction action, string? notes = null)
        {
            try
            {
                var accessLog = new DocumentAccessLog
                {
                    DocumentId = documentId,
                    UserId = userId,
                    Action = action,
                    AccessedAt = DateTime.UtcNow,
                    Notes = notes
                };

                _context.DocumentAccessLogs.Add(accessLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging document access for document {DocumentId}, user {UserId}", documentId, userId);
            }
        }
    }
}