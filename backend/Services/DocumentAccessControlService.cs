using backend.Models;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Document access control service with comprehensive permission management
    /// </summary>
    public class DocumentAccessControlService : IDocumentAccessControlService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DocumentAccessControlService> _logger;

        public DocumentAccessControlService(
            ApplicationDbContext context,
            ILogger<DocumentAccessControlService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Checks if a user can access a specific document
        /// </summary>
        public async Task<DocumentAccessResult> CanAccessDocumentAsync(int documentId, int userId, DocumentAccessAction action)
        {
            try
            {
                var document = await _context.Documents
                    .Include(d => d.UploadedBy)
                    .Include(d => d.Client)
                    .Include(d => d.SkillsDevelopmentProvider)
                    .Include(d => d.Department)
                    .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

                if (document == null)
                {
                    return new DocumentAccessResult
                    {
                        HasAccess = false,
                        Reason = "Document not found or has been deleted"
                    };
                }

                var user = await _context.Users
                    .Include(u => u.Client)
                    .Include(u => u.SkillsDevelopmentProvider)
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new DocumentAccessResult
                    {
                        HasAccess = false,
                        Reason = "User not found"
                    };
                }

                // Check if user is the document owner
                if (document.UploadedByUserId == userId)
                {
                    return new DocumentAccessResult
                    {
                        HasAccess = true,
                        Reason = "Document owner",
                        Permissions = GetOwnerPermissions()
                    };
                }

                // Check administrative access
                if (await HasAdministrativeAccessAsync(userId))
                {
                    return new DocumentAccessResult
                    {
                        HasAccess = true,
                        Reason = "Administrative access",
                        Permissions = GetAdminPermissions()
                    };
                }

                // Check explicit permissions
                var explicitPermissions = await GetUserPermissionsAsync(documentId, userId);
                if (HasRequiredPermission(explicitPermissions, action))
                {
                    return new DocumentAccessResult
                    {
                        HasAccess = true,
                        Reason = "Explicit permission granted",
                        Permissions = explicitPermissions
                    };
                }

                // Check access level based permissions
                var accessResult = await CheckAccessLevelPermissionsAsync(document, user, action);
                if (accessResult.HasAccess)
                {
                    return accessResult;
                }

                // Check hierarchical access
                if (await ValidateHierarchicalAccessAsync(document, user))
                {
                    var hierarchicalPermissions = GetHierarchicalPermissions(action);
                    return new DocumentAccessResult
                    {
                        HasAccess = true,
                        Reason = "Hierarchical access",
                        Permissions = hierarchicalPermissions
                    };
                }

                return new DocumentAccessResult
                {
                    HasAccess = false,
                    Reason = "Insufficient permissions"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking document access for document {DocumentId}, user {UserId}", documentId, userId);
                return new DocumentAccessResult
                {
                    HasAccess = false,
                    Reason = "Error checking permissions"
                };
            }
        }

        /// <summary>
        /// Checks if a user can perform a specific action on a document type
        /// </summary>
        public async Task<DocumentAccessResult> CanPerformActionAsync(DocumentType documentType, int userId, DocumentAccessAction action)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new DocumentAccessResult
                    {
                        HasAccess = false,
                        Reason = "User not found"
                    };
                }

                // Check administrative access
                if (await HasAdministrativeAccessAsync(userId))
                {
                    return new DocumentAccessResult
                    {
                        HasAccess = true,
                        Reason = "Administrative access",
                        Permissions = GetAdminPermissions()
                    };
                }

                // Check role-based permissions for document type
                var hasPermission = await CheckRoleBasedPermissionsAsync(user, documentType, action);
                
                return new DocumentAccessResult
                {
                    HasAccess = hasPermission,
                    Reason = hasPermission ? "Role-based permission" : "Insufficient role permissions"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking action permission for document type {DocumentType}, user {UserId}", documentType, userId);
                return new DocumentAccessResult
                {
                    HasAccess = false,
                    Reason = "Error checking permissions"
                };
            }
        }

        /// <summary>
        /// Gets all documents accessible by a user
        /// </summary>
        public async Task<List<Document>> GetAccessibleDocumentsAsync(int userId, DocumentType? documentType = null, DocumentAccessLevel? accessLevel = null)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Client)
                    .Include(u => u.SkillsDevelopmentProvider)
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new List<Document>();
                }

                var query = _context.Documents
                    .Include(d => d.UploadedBy)
                    .Where(d => !d.IsDeleted);

                // Apply document type filter
                if (documentType.HasValue)
                {
                    query = query.Where(d => d.Type == documentType.Value);
                }

                // Apply access level filter
                if (accessLevel.HasValue)
                {
                    query = query.Where(d => d.AccessLevel == accessLevel.Value);
                }

                var allDocuments = await query.ToListAsync();
                var accessibleDocuments = new List<Document>();

                foreach (var document in allDocuments)
                {
                    var accessResult = await CanAccessDocumentAsync(document.Id, userId, DocumentAccessAction.View);
                    if (accessResult.HasAccess)
                    {
                        accessibleDocuments.Add(document);
                    }
                }

                return accessibleDocuments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accessible documents for user {UserId}", userId);
                return new List<Document>();
            }
        }

        /// <summary>
        /// Grants access to a document for a specific user
        /// </summary>
        public async Task<DocumentAccessResult> GrantAccessAsync(int documentId, int userId, int grantedByUserId, DocumentPermissions permissions)
        {
            try
            {
                // Check if granter has permission to manage access
                var granterAccess = await CanAccessDocumentAsync(documentId, grantedByUserId, DocumentAccessAction.Share);
                if (!granterAccess.HasAccess || !granterAccess.Permissions.CanManageAccess)
                {
                    return new DocumentAccessResult
                    {
                        HasAccess = false,
                        Reason = "Insufficient permissions to grant access"
                    };
                }

                // Check if permission already exists
                var existingPermission = await _context.DocumentPermissions
                    .FirstOrDefaultAsync(dp => dp.DocumentId == documentId && dp.UserId == userId && dp.IsActive);

                if (existingPermission != null)
                {
                    // Update existing permission
                    existingPermission.CanView = permissions.CanView;
                    existingPermission.CanDownload = permissions.CanDownload;
                    existingPermission.CanEdit = permissions.CanEdit;
                    existingPermission.CanDelete = permissions.CanDelete;
                    existingPermission.CanShare = permissions.CanShare;
                    existingPermission.CanApprove = permissions.CanApprove;
                    existingPermission.CanManageAccess = permissions.CanManageAccess;
                    existingPermission.ExpiresAt = permissions.ExpiresAt;
                    existingPermission.GrantedByUserId = grantedByUserId;
                    existingPermission.GrantedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new permission
                    var newPermission = new DocumentPermission
                    {
                        DocumentId = documentId,
                        UserId = userId,
                        CanView = permissions.CanView,
                        CanDownload = permissions.CanDownload,
                        CanEdit = permissions.CanEdit,
                        CanDelete = permissions.CanDelete,
                        CanShare = permissions.CanShare,
                        CanApprove = permissions.CanApprove,
                        CanManageAccess = permissions.CanManageAccess,
                        ExpiresAt = permissions.ExpiresAt,
                        GrantedByUserId = grantedByUserId,
                        GrantedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.DocumentPermissions.Add(newPermission);
                }

                await _context.SaveChangesAsync();

                return new DocumentAccessResult
                {
                    HasAccess = true,
                    Reason = "Access granted successfully",
                    Permissions = permissions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting access to document {DocumentId} for user {UserId}", documentId, userId);
                return new DocumentAccessResult
                {
                    HasAccess = false,
                    Reason = "Error granting access"
                };
            }
        }

        /// <summary>
        /// Revokes access to a document for a specific user
        /// </summary>
        public async Task<DocumentAccessResult> RevokeAccessAsync(int documentId, int userId, int revokedByUserId)
        {
            try
            {
                // Check if revoker has permission to manage access
                var revokerAccess = await CanAccessDocumentAsync(documentId, revokedByUserId, DocumentAccessAction.Share);
                if (!revokerAccess.HasAccess || !revokerAccess.Permissions.CanManageAccess)
                {
                    return new DocumentAccessResult
                    {
                        HasAccess = false,
                        Reason = "Insufficient permissions to revoke access"
                    };
                }

                var permission = await _context.DocumentPermissions
                    .FirstOrDefaultAsync(dp => dp.DocumentId == documentId && dp.UserId == userId && dp.IsActive);

                if (permission != null)
                {
                    permission.IsActive = false;
                    permission.RevokedAt = DateTime.UtcNow;
                    permission.RevokedByUserId = revokedByUserId;

                    await _context.SaveChangesAsync();
                }

                return new DocumentAccessResult
                {
                    HasAccess = true,
                    Reason = "Access revoked successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking access to document {DocumentId} for user {UserId}", documentId, userId);
                return new DocumentAccessResult
                {
                    HasAccess = false,
                    Reason = "Error revoking access"
                };
            }
        }

        /// <summary>
        /// Gets user permissions for a specific document
        /// </summary>
        public async Task<DocumentPermissions> GetUserPermissionsAsync(int documentId, int userId)
        {
            try
            {
                var permission = await _context.DocumentPermissions
                    .Include(dp => dp.GrantedBy)
                    .FirstOrDefaultAsync(dp => dp.DocumentId == documentId && dp.UserId == userId && dp.IsActive);

                if (permission == null)
                {
                    return new DocumentPermissions();
                }

                // Check if permission has expired
                if (permission.ExpiresAt.HasValue && permission.ExpiresAt.Value < DateTime.UtcNow)
                {
                    return new DocumentPermissions();
                }

                return new DocumentPermissions
                {
                    CanView = permission.CanView,
                    CanDownload = permission.CanDownload,
                    CanEdit = permission.CanEdit,
                    CanDelete = permission.CanDelete,
                    CanShare = permission.CanShare,
                    CanApprove = permission.CanApprove,
                    CanManageAccess = permission.CanManageAccess,
                    ExpiresAt = permission.ExpiresAt,
                    GrantedBy = permission.GrantedBy.Email,
                    GrantedAt = permission.GrantedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions for document {DocumentId}, user {UserId}", documentId, userId);
                return new DocumentPermissions();
            }
        }

        /// <summary>
        /// Checks if user has administrative access to documents
        /// </summary>
        public async Task<bool> HasAdministrativeAccessAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                return user?.Role == UserRole.SystemAdmin || user?.Role == UserRole.ClientAdmin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking administrative access for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Validates document access based on organizational hierarchy
        /// </summary>
        public async Task<bool> ValidateHierarchicalAccessAsync(Document document, User user)
        {
            try
            {
                // Same organization access
                if (document.ClientId.HasValue && document.ClientId == user.ClientId)
                {
                    return true;
                }

                if (document.SkillsDevelopmentProviderId.HasValue && 
                    document.SkillsDevelopmentProviderId == user.SkillsDevelopmentProviderId)
                {
                    return true;
                }

                if (document.DepartmentId.HasValue && document.DepartmentId == user.DepartmentId)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating hierarchical access for document {DocumentId}, user {UserId}", 
                    document.Id, user.Id);
                return false;
            }
        }

        /// <summary>
        /// Checks access level based permissions
        /// </summary>
        private async Task<DocumentAccessResult> CheckAccessLevelPermissionsAsync(Document document, User user, DocumentAccessAction action)
        {
            switch (document.AccessLevel)
            {
                case DocumentAccessLevel.Public:
                    return new DocumentAccessResult
                    {
                        HasAccess = true,
                        Reason = "Public access",
                        Permissions = GetPublicPermissions()
                    };

                case DocumentAccessLevel.Organization:
                    if (await ValidateHierarchicalAccessAsync(document, user))
                    {
                        return new DocumentAccessResult
                        {
                            HasAccess = true,
                            Reason = "Organization access",
                            Permissions = GetOrganizationPermissions()
                        };
                    }
                    break;

                case DocumentAccessLevel.Department:
                    if (document.DepartmentId == user.DepartmentId)
                    {
                        return new DocumentAccessResult
                        {
                            HasAccess = true,
                            Reason = "Department access",
                            Permissions = GetDepartmentPermissions()
                        };
                    }
                    break;

                case DocumentAccessLevel.Client:
                    if (document.ClientId == user.ClientId)
                    {
                        return new DocumentAccessResult
                        {
                            HasAccess = true,
                            Reason = "Client access",
                            Permissions = GetClientPermissions()
                        };
                    }
                    break;
            }

            return new DocumentAccessResult
            {
                HasAccess = false,
                Reason = "Access level restrictions"
            };
        }

        /// <summary>
        /// Checks role-based permissions for document type
        /// </summary>
        private async Task<bool> CheckRoleBasedPermissionsAsync(User user, DocumentType documentType, DocumentAccessAction action)
        {
            // System admins can do everything
            if (user.Role == UserRole.SystemAdmin)
                return true;

            // Admins can manage most document types
            if (user.Role == UserRole.SystemAdmin || user.Role == UserRole.ClientAdmin)
            {
                return documentType != DocumentType.Identity || action != DocumentAccessAction.Delete;
            }

            // Managers can view and approve documents
            if (user.Role == UserRole.SDPAdministrator || user.Role == UserRole.SDPModerator)
            {
                return action == DocumentAccessAction.View || action == DocumentAccessAction.Approve;
            }

            // Regular users can upload and view their own documents
            if (user.Role == UserRole.Learner)
            {
                return action == DocumentAccessAction.Upload || action == DocumentAccessAction.View;
            }

            return false;
        }

        /// <summary>
        /// Checks if user has required permission for action
        /// </summary>
        private bool HasRequiredPermission(DocumentPermissions permissions, DocumentAccessAction action)
        {
            return action switch
            {
                DocumentAccessAction.View => permissions.CanView,
                DocumentAccessAction.Download => permissions.CanDownload,
                DocumentAccessAction.Update => permissions.CanEdit,
                DocumentAccessAction.Delete => permissions.CanDelete,
                DocumentAccessAction.Share => permissions.CanShare,
                DocumentAccessAction.Approve => permissions.CanApprove,
                _ => false
            };
        }

        /// <summary>
        /// Gets owner permissions (full access)
        /// </summary>
        private DocumentPermissions GetOwnerPermissions()
        {
            return new DocumentPermissions
            {
                CanView = true,
                CanDownload = true,
                CanEdit = true,
                CanDelete = true,
                CanShare = true,
                CanApprove = false,
                CanManageAccess = true
            };
        }

        /// <summary>
        /// Gets admin permissions (full access including approval)
        /// </summary>
        private DocumentPermissions GetAdminPermissions()
        {
            return new DocumentPermissions
            {
                CanView = true,
                CanDownload = true,
                CanEdit = true,
                CanDelete = true,
                CanShare = true,
                CanApprove = true,
                CanManageAccess = true
            };
        }

        /// <summary>
        /// Gets public access permissions (view only)
        /// </summary>
        private DocumentPermissions GetPublicPermissions()
        {
            return new DocumentPermissions
            {
                CanView = true,
                CanDownload = false,
                CanEdit = false,
                CanDelete = false,
                CanShare = false,
                CanApprove = false,
                CanManageAccess = false
            };
        }

        /// <summary>
        /// Gets organization access permissions
        /// </summary>
        private DocumentPermissions GetOrganizationPermissions()
        {
            return new DocumentPermissions
            {
                CanView = true,
                CanDownload = true,
                CanEdit = false,
                CanDelete = false,
                CanShare = false,
                CanApprove = false,
                CanManageAccess = false
            };
        }

        /// <summary>
        /// Gets department access permissions
        /// </summary>
        private DocumentPermissions GetDepartmentPermissions()
        {
            return new DocumentPermissions
            {
                CanView = true,
                CanDownload = true,
                CanEdit = false,
                CanDelete = false,
                CanShare = true,
                CanApprove = false,
                CanManageAccess = false
            };
        }

        /// <summary>
        /// Gets client access permissions
        /// </summary>
        private DocumentPermissions GetClientPermissions()
        {
            return new DocumentPermissions
            {
                CanView = true,
                CanDownload = true,
                CanEdit = false,
                CanDelete = false,
                CanShare = true,
                CanApprove = false,
                CanManageAccess = false
            };
        }

        /// <summary>
        /// Gets hierarchical access permissions
        /// </summary>
        private DocumentPermissions GetHierarchicalPermissions(DocumentAccessAction action)
        {
            return new DocumentPermissions
            {
                CanView = true,
                CanDownload = action != DocumentAccessAction.Delete,
                CanEdit = false,
                CanDelete = false,
                CanShare = false,
                CanApprove = false,
                CanManageAccess = false
            };
        }
    }
}