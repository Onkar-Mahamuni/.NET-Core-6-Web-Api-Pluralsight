using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService, ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _logger= logger ?? throw new ArgumentNullException(nameof(logger)); //Adding a null check
            // _logger = HttpContext.RequestServices.GetService(); > Alternate way of doing it
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _cityInfoRepository = cityInfoRepository  ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
        //We should always send and receive data to client through DTOs not to disclose the entity structure
        {
            try
            {
                //// for trial purposes
                //throw new Exception("Exception sample");


                if (! await _cityInfoRepository.CityExistsAsync(cityId))
                {
                    _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest");
                    return NotFound();
                }
                var pointsOfInterestForCity = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);

                return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity));
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting points of interest for city with id {cityId}", ex);
                // Never return a stack trace to the consumer as the internal information can become vulnerable
                return StatusCode(500, "A problem happened while handling your request");
            }
        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId, int pointofinterestid)
        {
            //var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var pointOfInterest = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointofinterestid);

            //var PointOfInterest = city.PointsOfInterest.FirstOrDefault( c => c.Id == pointofinterestid);

            if (pointOfInterest == null)
            {
                return NotFound();
            }

            //return Ok(PointOfInterest);
            return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterest));

        }

        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest)
        {
            //if(!ModelState.IsValid)
            //{
            //    return BadRequest();
            //} //Optional as it is already done automatically by ModelState and APIController attribute

            //var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
            //if (cityId == null)
            //{
            //    return NotFound();
            //}

            if(!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var finalPointOfInterest = _mapper.Map<PointOfInterest>(pointOfInterest);

            //// Demo porposes - to be improved
            //var maxPointOfInterestId = _citiesDataStore.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);

            //var finalPointOfInterest = new PointOfInterestDto()
            //{
            //    Id = ++maxPointOfInterestId,
            //    Name = pointOfInterest.Name,
            //    Description = pointOfInterest.Description
            //};

            await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);
            await _cityInfoRepository.SaveChangesAsync();

            var createdPointOfInterestToReturn = _mapper.Map<PointOfInterestDto>(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                new
                {
                    cityId = cityId,
                    pointOfInterestId = createdPointOfInterestToReturn.Id
                }, createdPointOfInterestToReturn);
        }

        [HttpPut("{pointofinterestid}")] //For full updates
        public async Task<ActionResult<PointOfInterestDto>> UpdatePointOfInterest(int cityId, int pointOfInterestId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            //if(!ModelState.IsValid)
            //{
            //    return BadRequest();
            //} //Optional as it is already done automatically by ModelState and APIController attribute

            if(!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }
            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);

            if(pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(pointOfInterest, pointOfInterestEntity);

            await _cityInfoRepository.SaveChangesAsync();

            //if (city == null)
            //{
            //    return NotFound();
            //}

            //var PointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(c => c.Id == pointOfInterestId);

            //if (PointOfInterestFromStore == null)
            //{
            //    return NotFound();
            //}


            //PointOfInterestFromStore.Name = pointOfInterest.Name;
            //PointOfInterestFromStore.Description = pointOfInterest.Description;

            return NoContent();
        }



        [HttpPatch("{pointofinterestid}")] //For full updates
        public async Task<ActionResult<PointOfInterestDto>> PartiallyUpdatePointOfInterest(int cityId, int pointOfInterestId, [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            //var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

            //if (city == null)
            //{
            //    return NotFound();
            //}

            if(!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            //var PointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(c => c.Id == pointOfInterestId);

            //if (PointOfInterestFromStore == null)
            //{
            //    return NotFound();
            //}

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if(pointOfInterestEntity == null)
            {
                NotFound();
            }


            //var pointOfInterestToPatch =
            //    new PointOfInterestForUpdateDto()
            //    {
            //        Name = PointOfInterestFromStore.Name,
            //        Description = PointOfInterestFromStore.Description
            //    };

            var pointOfInterestToPatch = _mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            } //Compulsory as here apicontroller annotation does not interfere

            if (!TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

            await _cityInfoRepository.SaveChangesAsync();
            //PointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
            //PointOfInterestFromStore.Description = pointOfInterestToPatch.Description;

            return NoContent();
        }

        [HttpDelete("{pointofinterestid}")]
        public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            //var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

            //if (city == null)
            //{
            //    return NotFound();
            //}

            if(!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);

            //var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(c => c.Id == pointOfInterestId);
            //if (pointOfInterestFromStore == null)
            //{
            //    return NotFound();
            //}

            if(pointOfInterestEntity == null)
            {
                return NotFound();
            }

            //city.PointsOfInterest.Remove(pointOfInterestFromStore);
            _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);

            _mailService.Send("Point of interest deleted.", $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id}"); //(Subject of mail, Mail Body)
            return NoContent();
        }

    }
}
