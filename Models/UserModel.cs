using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Base.Api.Models
{
    public class UserModel
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("user_guid")]
        public Guid UserGuid { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        [Column("username")]
        public required string Username { get; set; }

        [Required]
        [Column("password_hash")]
        public required string PasswordHash { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("contact_no")]
        public string? ContactNo { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("failed_login_count")]
        public int FailedLoginCount { get; set; } = 0;

        [Column("last_failed_login")]
        public DateTime? LastFailedLogin { get; set; }

        [Column("locked_until")]
        public DateTime? LockedUntil { get; set; }

        [Column("is_online")]
        public bool IsOnline { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_by")]
        public int UpdatedBy { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("deleted_by")]
        public string? DeletedBy { get; set; }
    }
}
