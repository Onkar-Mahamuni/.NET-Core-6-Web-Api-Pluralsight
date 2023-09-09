using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CityInfo.API.Entities
{
    public class City
    {
        [Key] //Optional if the name is Id or CityId etc
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //Identity: Gerate auto by adding
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public ICollection<PointOfInterest> pointOfInterest { get; set; } = new List<PointOfInterest>();

        public City(string name) //We convery that city must always have a name
        {
            Name = name;
        }
    }
}
