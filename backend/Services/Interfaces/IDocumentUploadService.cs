using backend.Models;
using Microsoft.AspNetCore.Http;

namespace backend.Services.Interfaces
{
    /// <summary>
    /// Interface for secure document upload service with comprehensive validation
    /// </summary>
    public interface IDocumentUploadService
    {
        /// <summary>
        /// Uploads a document with security validation and encryption
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <param name="uploadRequest">Upload request with metadata</param>
        /// <param name="userId">ID of the user uploading the document</param>
        /// <returns>Upload result with document information</returns>
        Task<DocumentUploadResult> UploadDocumentAsync(IFormFile file, DocumentUploadRequest uploadRequest, int userId);

        /// <summary>
        /// Validates file before upload
        /// </summary>
        /// <param name="file">The file to validate</param>
        /// <param name="documentType">Type of document being uploaded</param>
        /// <returns>Validation result</returns>
        Task<FileValidationResult> ValidateFileAsync(IFormFile file, DocumentType documentType);

        /// <summary>
        /// Gets allowed file types for a specific document type
        /// </summary>
        /// <param name="documentType">Document type</param>
        /// <returns>List of allowed MIME types</returns>
        List<string> GetAllowedFileTypes(DocumentType documentType);

        /// <summary>
        /// Gets maximum file size for a specific document type
        /// </summary>
        /// <param name="documentType">Document type</param>
        /// <returns>Maximum file size in bytes</returns>
        long GetMaxFileSize(DocumentType documentType);

        /// <summary>
        /// Scans file for malware (placeholder for future implementation)
        /// </summary>
        /// <param name="filePath">Path to the file to scan</param>
        /// <returns>Scan result</returns>
        Task<MalwareScanResult> ScanFileForMalwareAsync(string filePath);

        /// <summary>
        /// Generates secure file name to prevent path traversal attacks
        /// </summary>
        /// <param name="originalFileName">Original file name</param>
        /// <param name="documentType">Document type</param>
        /// <returns>Secure file name</returns>
        string GenerateSecureFileName(string originalFileName, DocumentType documentType);

        /// <summary>
        /// Creates secure storage directory structure
        /// </summary>
        /// <param name="documentType">Document type</param>
        /// <param name="userId">User ID</param>
        /// <returns>Secure directory path</returns>
        string CreateSecureStorageDirectory(DocumentType documentType, int userId);
    }

    /// <summary>
    /// Document upload request model
    /// </summary>
    public class DocumentUploadRequest
    {
        public IFormFile File { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public string? Description { get; set; }
        public DocumentAccessLevel AccessLevel { get; set; } = DocumentAccessLevel.Private;
        public bool RequiresApproval { get; set; } = false;
        public DateTime? ExpiresAt { get; set; }
        public int? ClientId { get; set; }
        public int? SkillsDevelopmentProviderId { get; set; }
        public int? DepartmentId { get; set; }
        public int UploadedByUserId { get; set; }
    }

    /// <summary>
    /// Document upload result
    /// </summary>
    public class DocumentUploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Document? Document { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public FileValidationResult? ValidationResult { get; set; }
        public MalwareScanResult? ScanResult { get; set; }
    }

    /// <summary>
    /// File validation result
    /// </summary>
    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public string DetectedMimeType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileExtension { get; set; } = string.Empty;
    }

    /// <summary>
    /// Malware scan result (placeholder for future implementation)
    /// </summary>
    public class MalwareScanResult
    {
        public bool IsClean { get; set; } = true;
        public string ScanEngine { get; set; } = "NotImplemented";
        public DateTime ScanDate { get; set; } = DateTime.UtcNow;
        public List<string> Threats { get; set; } = new List<string>();
        public string ScanId { get; set; } = string.Empty;
    }
}