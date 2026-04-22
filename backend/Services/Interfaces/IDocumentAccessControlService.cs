using backend.Models;

namespace backend.Services.Interfaces
{
    /// <summary>
    /// Interface for document access control and permission management
    /// </summary>
    public interface IDocumentAccessControlService
    {
        /// <summary>
        /// Checks if a user can access a specific document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="action">Requested action</param>
        /// <returns>Access result with permission details</returns>
        Task<DocumentAccessResult> CanAccessDocumentAsync(int documentId, int userId, DocumentAccessAction action);

        /// <summary>
        /// Checks if a user can perform a specific action on a document type
        /// </summary>
        /// <param name="documentType">Document type</param>
        /// <param name="userId">User ID</param>
        /// <param name="action">Requested action</param>
        /// <returns>Access result with permission details</returns>
        Task<DocumentAccessResult> CanPerformActionAsync(DocumentType documentType, int userId, DocumentAccessAction action);

        /// <summary>
        /// Gets all documents accessible by a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="documentType">Optional document type filter</param>
        /// <param name="accessLevel">Optional access level filter</param>
        /// <returns>List of accessible documents</returns>
        Task<List<Document>> GetAccessibleDocumentsAsync(int userId, DocumentType? documentType = null, DocumentAccessLevel? accessLevel = null);

        /// <summary>
        /// Grants access to a document for a specific user
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="userId">User ID to grant access to</param>
        /// <param name="grantedByUserId">User ID who is granting access</param>
        /// <param name="permissions">Permissions to grant</param>
        /// <returns>Result of the grant operation</returns>
        Task<DocumentAccessResult> GrantAccessAsync(int documentId, int userId, int grantedByUserId, DocumentPermissions permissions);

        /// <summary>
        /// Revokes access to a document for a specific user
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="userId">User ID to revoke access from</param>
        /// <param name="revokedByUserId">User ID who is revoking access</param>
        /// <returns>Result of the revoke operation</returns>
        Task<DocumentAccessResult> RevokeAccessAsync(int documentId, int userId, int revokedByUserId);

        /// <summary>
        /// Gets user permissions for a specific document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="userId">User ID</param>
        /// <returns>User's permissions for the document</returns>
        Task<DocumentPermissions> GetUserPermissionsAsync(int documentId, int userId);

        /// <summary>
        /// Checks if user has administrative access to documents
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if user has admin access</returns>
        Task<bool> HasAdministrativeAccessAsync(int userId);

        /// <summary>
        /// Validates document access based on organizational hierarchy
        /// </summary>
        /// <param name="document">Document to check</param>
        /// <param name="user">User requesting access</param>
        /// <returns>True if access is allowed based on hierarchy</returns>
        Task<bool> ValidateHierarchicalAccessAsync(Document document, User user);
    }

    /// <summary>
    /// Document access result
    /// </summary>
    public class DocumentAccessResult
    {
        public bool HasAccess { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DocumentPermissions Permissions { get; set; } = new DocumentPermissions();
        public List<string> RequiredRoles { get; set; } = new List<string>();
        public bool RequiresApproval { get; set; }
    }

    /// <summary>
    /// Document permissions for a user
    /// </summary>
    public class DocumentPermissions
    {
        public bool CanView { get; set; }
        public bool CanDownload { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanShare { get; set; }
        public bool CanApprove { get; set; }
        public bool CanManageAccess { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string GrantedBy { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Document permission entity for database storage
    /// </summary>
    public class DocumentPermission
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public int UserId { get; set; }
        public bool CanView { get; set; } = true;
        public bool CanDownload { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool CanShare { get; set; } = false;
        public bool CanApprove { get; set; } = false;
        public bool CanManageAccess { get; set; } = false;
        public DateTime? ExpiresAt { get; set; }
        public int GrantedByUserId { get; set; }
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public DateTime? RevokedAt { get; set; }
        public int? RevokedByUserId { get; set; }

        // Navigation Properties
        public virtual Document Document { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual User GrantedBy { get; set; } = null!;
        public virtual User? RevokedBy { get; set; }
    }
}