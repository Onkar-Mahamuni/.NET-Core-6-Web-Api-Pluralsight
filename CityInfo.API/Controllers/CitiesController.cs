using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private ICityInfoRepository _cityInfoRepository;

        public CitiesController(ICityInfoRepository cityInfoRepository) 
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDTO>>> GetCities()
        {
            var cityEntities = await _cityInfoRepository.GetCitiesAsync();
            var results = new List<CityWithoutPointsOfInterestDTO>();
            foreach (var city in cityEntities)
            {
                results.Add(new CityWithoutPointsOfInterestDTO()
                {
                    Id = city.Id,
                    Description = city.Description,
                    Name = city.Name
                });
            }

            return Ok(results);
            //return Ok(_citiesDataStore.Cities);
        }

        //[HttpGet("{id}")]
        //public ActionResult<CityDto> GetCity(int id)
        //{
        //    var cityToReturn = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == id);
        //    if(cityToReturn == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(cityToReturn);

        //    //return new JsonResult(
        //    //    CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == id)
        //    //);
        //}
    }
}
