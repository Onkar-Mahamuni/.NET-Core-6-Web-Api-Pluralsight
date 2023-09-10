using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;

namespace CityInfo.API.Profiles
{
    public class CityProfile : Profile
    {
        public CityProfile()
        {
            CreateMap<City, CityWithoutPointsOfInterestDTO>();
            //The property if doesnt exists, will be ignored
            CreateMap<City, CityDto>();
        }
    }
}
