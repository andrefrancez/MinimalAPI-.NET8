using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace project.Domain.Entities
{
    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Make { get; set; }

        [Required]
        public string ModelYear { get; set; }

        [Required]
        [StringLength(70)]
        public string Color { get; set; }

        [StringLength(250)]
        public string Description { get; set; }
    }
}
