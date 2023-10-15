using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")] //We can give versioning support for multiple versions this way
    [Route("api/v{version:apiVersion}/cities")]
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

        // Summary can be added by typing "///" (Forward Slash 3 times)
        // If the summary is not working, go to project properties > Debug > Output > Tick Documentation File, then in Program.cs pass set of actions to AddSwaggerGen() to configure 
        /// <summary>
        /// Get a city by id
        /// </summary>
        /// <param name="id">The id of the city to get</param>
        /// <param name="includePointsOfInterest">Whether or not to include the points of interest</param>
        /// <returns>An IActionResult</returns>
        //To override reponse status code's description: 
        /// <response code="200">Returns the requested city</response>
        /// <response code="404">Requested resource is not found</response>
        /// <response code="400">Bad request from client</response>
        /// 

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)] //This is to document potential responses 
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

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
