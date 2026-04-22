using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DepartmentType Type { get; set; }

        [Required]
        public DepartmentStatus Status { get; set; } = DepartmentStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        [Required]
        public int SkillsDevelopmentProviderId { get; set; }

        // Navigation Properties
        [ForeignKey("SkillsDevelopmentProviderId")]
        public virtual SkillsDevelopmentProvider SkillsDevelopmentProvider { get; set; } = null!;

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }

    public enum DepartmentType
    {
        Administration = 1,
        Finance = 2,
        Logistics = 3,
        IT = 4,
        Moderation = 5,
        Assessment = 6,
        Facilitation = 7,
        Learning = 8
    }

    public enum DepartmentStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3
    }
}