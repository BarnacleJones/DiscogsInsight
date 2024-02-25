using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using DiscogsInsight.ApiIntegration.Services;
using DiscogsInsight.DataAccess.Entities;

namespace DiscogsInsight.DataAccess.Services
{
    public class ReleaseDataService
    {
        private readonly DiscogsInsightDb _db;
        private readonly DiscogsApiService _discogsApiService;
        private readonly CollectionDataService _collectionDataService;

        public ReleaseDataService(DiscogsInsightDb db, DiscogsApiService discogsApiService, CollectionDataService collectionDataService)
        {
            _db = db;
            _discogsApiService = discogsApiService;
            _collectionDataService = collectionDataService;
        }

        public async Task<Release?> GetRelease(int? discogsReleaseId)
        {
            if (discogsReleaseId == null)
                throw new Exception($"Missing discogs release id");

            var releases = await _db.GetAllEntitiesAsync<Release>();
            var release = releases.FirstOrDefault(x => x.DiscogsReleaseId == discogsReleaseId);

            //if release is null save it and the tracks here
            return release;
        }

        public async Task<Release> GetRandomRelease()
        {
            var releases = await _db.GetAllEntitiesAsync<Release>();
            if (releases.Count < 1)
            {
                releases = await _collectionDataService.GetReleases();
            }

            var randomRelease = releases.OrderBy(r => Guid.NewGuid()).FirstOrDefault();//new GUID as key, will be random

            if (randomRelease is null)
            {
                throw new Exception($"Error getting random release.");
            }

            return randomRelease;          
        }

        public async Task<List<Release>> GetNewestReleases(int howManyToReturn)
        {
            var returnedReleases = new List<Release>();

            var releases = await _db.GetAllEntitiesAsync<Release>();
            if (releases.Count < 1)
            {
                releases = await _collectionDataService.GetReleases();
            }
            var releaseIsLargerThanParameter = releases.Count() >= howManyToReturn;

            return releases.OrderByDescending(r => r.DateAdded)
                           .Take(releaseIsLargerThanParameter ? howManyToReturn : 1)
                           .ToList();
        }

    }
}
