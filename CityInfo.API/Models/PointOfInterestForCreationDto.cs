
using System.ComponentModel.DataAnnotations;

namespace CityInfo.API.Models
{
    public class PointOfInterestForCreationDto
    {
        [Required(ErrorMessage ="Name must be provided!")]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        // Annotations mix validation rules with model which is not a good separation of concerns but it is okay for small to medium size of utilities
        // For complex utilities we need an alternative like FluentValidation
    }
}
