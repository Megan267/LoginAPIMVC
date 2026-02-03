using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)] //so that the password is not shown
        public string Password { get; set; }
    }
}
