using project.Domain.Enums;

namespace project.Domain.DTOs
{
    public class AdminDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public Profile? Profile { get; set; }
    }
}
