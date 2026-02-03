using System.ComponentModel.DataAnnotations;

namespace API_Pets.Model
{
    public class Pets
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Breed { get; set; }

        [Required]
        [StringLength(255)]
        public string ImageUrl { get; set; } = "https://picsum.photos/200/300";

    }
}
