using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class SkillsDevelopmentProvider
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(50)]
        public string? RegistrationNumber { get; set; }

        [Required]
        public SDPStatus Status { get; set; } = SDPStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        [Required]
        public int ClientId { get; set; }

        // Navigation Properties
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; } = null!;

        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    }

    public enum SDPStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        PendingApproval = 4
    }
}