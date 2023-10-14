using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using SQLitePCL;

namespace CityInfo.API.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private CityInfoContext _context;

        public CityInfoRepository(CityInfoContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            return await _context.Cities.OrderBy(c=>c.Name).ToListAsync();
        }

        public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? searchQuery, int pageNumber, int pageSize)
        {
            // Method is inappropriate after pagination
            //if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(searchQuery))
            //{
            //    return await GetCitiesAsync();
            //}

            // collection to start from
            var collection = _context.Cities as IQueryable<City>;

            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim();
                collection = collection.Where(c=>c.Name == name);
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                collection = collection.Where(a => a.Name.Contains(searchQuery) || a.Description != null && a.Description.Contains(searchQuery));
            }

            // It is important to build the query first and then executing it rather than executing it multiple times
            // It is known as Deferred Execution
            // IQueryable<T> : creates an expression tree
            // Execution is deferred until the query is iterated over like -foreach loop, - ToList(),ToArray(),ToDictionary(), -Singleton queries

            var totalItemsCount = await collection.CountAsync();

            var paginationMetadata = new PaginationMetadata(totalItemsCount, pageSize, pageNumber);

            var collectionToReturn = await collection.OrderBy(c => c.Name)
            //It is important to add pagination query at last because otherwise search and filter will only happen on data of perticular page
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (collectionToReturn, paginationMetadata);

            //name = name.Trim();
            //return await _context.Cities
            //    .Where(c => c.Name == name)
            //    .OrderBy(c =>c.Name)
            //    .ToListAsync();
        }

        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
        {
            if(includePointsOfInterest)
            {
                return await _context.Cities.Include(c=> c.pointOfInterest).Where(c=>c.Id==cityId).FirstOrDefaultAsync();
            }

            return await _context.Cities.Where(c => c.Id == cityId).FirstOrDefaultAsync();
        }

        public async Task<bool> CityExistsAsync(int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.Id == cityId);
        }

        public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId)
        {
            return await _context.PointOfInterests.Where(p => p.CityId == cityId && p.Id == pointOfInterestId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
        {
            return await _context.PointOfInterests.Where(p => p.CityId == cityId).ToListAsync();
        }

        public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId, false);
            if(city != null)
            {
                city.pointOfInterest.Add(pointOfInterest);
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            _context.PointOfInterests.Remove(pointOfInterest);
        }
    }
}
