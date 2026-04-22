using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace backend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password hash is required")]
        [StringLength(255, ErrorMessage = "Password hash cannot exceed 255 characters")]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\+]?[0-9\s\-\(\)]+$", ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public UserStatus Status { get; set; } = UserStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int? ClientId { get; set; }
        public int? SkillsDevelopmentProviderId { get; set; }
        public int? DepartmentId { get; set; }

        // Navigation Properties
        [ForeignKey("ClientId")]
        public virtual Client? Client { get; set; }

        [ForeignKey("SkillsDevelopmentProviderId")]
        public virtual SkillsDevelopmentProvider? SkillsDevelopmentProvider { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }
    }

    public enum UserRole
    {
        SystemAdmin = 1,
        ClientAdmin = 2,
        SDPAdministrator = 3,
        SDPFinance = 4,
        SDPLogistics = 5,
        SDPIT = 6,
        SDPModerator = 7,
        SDPAssessor = 8,
        SDPFacilitator = 9,
        Learner = 10
    }

    public enum UserStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        Pending = 4
    }
}