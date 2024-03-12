using System.ComponentModel.DataAnnotations;

namespace project.Domain.DTOs
{
    public record VehicleDTO
    {
        public string Name { get; set; }

        public string Make { get; set; }

        public int ModelYear { get; set; }

        public string Color { get; set; }

        public string Description { get; set; }
    }
}
