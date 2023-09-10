using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using System.Runtime.CompilerServices;

namespace CityInfo.API.Profiles
{
    public class PointsOfInterestProfile : Profile
    {
        public PointsOfInterestProfile()
        {
            CreateMap<PointOfInterest, PointOfInterestDto>();
            CreateMap<PointOfInterestForCreationDto, PointOfInterest>();
            CreateMap<PointOfInterestForUpdateDto, PointOfInterest>();
            CreateMap<PointOfInterest, PointOfInterestForUpdateDto>();
        }
    }
}
