using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private ICityInfoRepository _cityInfoRepository;
        private IMapper _mapper;
        const int maxCitiesPageSize = 20;
        // Declaring here because each entity can have different maximum page size

        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper) 
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        //public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDTO>>> GetCities([FromQuery(Name = "filteronname")] string? name)
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDTO>>> GetCities([FromQuery] string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > maxCitiesPageSize)
            {
                pageSize = maxCitiesPageSize;
            }

            //var results = new List<CityWithoutPointsOfInterestDTO>();
            //foreach (var city in cityEntities)
            //{
            //    results.Add(new CityWithoutPointsOfInterestDTO()
            //    {
            //        Id = city.Id,
            //        Description = city.Description,
            //        Name = city.Name
            //    });
            //}
            //return Ok(results);
            
            var (cityEntities, paginationMetadata) = await _cityInfoRepository.GetCitiesAsync(name, searchQuery, pageNumber, pageSize);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDTO>>(cityEntities));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCity(int id, bool includePointsOfInterest = false)
        {
            var city = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);
            if (city == null)
            {
                return NotFound();
            }

            if (includePointsOfInterest)
            {
                return Ok(_mapper.Map<CityDto>(city));
            }

            return Ok(_mapper.Map<CityWithoutPointsOfInterestDTO>(city));

            //return new JsonResult(
            //    CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == id)
            //);
        }
    }
}
