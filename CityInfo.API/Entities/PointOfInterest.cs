using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CityInfo.API.Entities
{
    public class PointOfInterest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //Better to apply the validations at the lowest level to ensure best possible data integration
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        [ForeignKey("CityId")] // Not required to add forgign key explicitly as the datatype is of custom type 
        public City? City { get; set; }

        //Optional - Navigational property
        public int CityId { get; set; }

        public PointOfInterest(string name) { Name = name; }
    }
}