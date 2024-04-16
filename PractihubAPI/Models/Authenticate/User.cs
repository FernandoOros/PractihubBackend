using System.ComponentModel.DataAnnotations;

namespace PractihubAPI.Models.Authenticate
{
    public class User
    {
        [Key]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }

    }
}
