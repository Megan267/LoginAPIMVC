using System.ComponentModel.DataAnnotations;

namespace API_Pets.Model
{
    public class User
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        [StringLength(50)]
        public string Role { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    }
}
