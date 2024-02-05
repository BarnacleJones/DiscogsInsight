using DiscogsInsight.DataModels;
using DiscogsInsight.ViewModels;

namespace DiscogsInsight.Services
{
    public class CollectionService
    {
        private readonly DiscogsInsightDb _db;
        private readonly DiscogsApiService _discogsApiService;

        public CollectionService(DiscogsInsightDb db, DiscogsApiService discogsApiService)
        { 
            _db = db;
            _discogsApiService = discogsApiService;
        }

        public async Task<DiscogsCollection> GetCollection()
        {
            List<Release> releaseList;
            var releases = await _db.GetAllEntitiesAsync<Release>();
            releaseList = releases.ToList();

            if (!releaseList.Any())
            {
                releaseList = await _discogsApiService.GetCollectionFromDiscogsAndSaveAndReturn();
                
            }
            var artists = await _db.GetAllEntitiesAsync<Artist>();
            var artistIdsList = artists.Select(x => new { x.DiscogsArtistId, x.Name }).ToList();

            var viewModel = releaseList.Select(x => new ReleaseViewModel
            {
                Artist = artistIdsList.Where(y => y.DiscogsArtistId == x.ArtistId).Select(x => x.Name).FirstOrDefault() ?? "ERROR",//Todo: make this better
                Year = x.Year,
                Title = x.Title,
                ResourceUrl = x.ResourceUrl,
                MasterUrl = x.MasterUrl,
                Genres = x.Genres,
                DateAdded = x.DateAdded
            }).ToList();

            return new DiscogsCollection { Releases = viewModel };
        }

        public async Task<ReleaseViewModel> GetReleaseViewModel()
        {
            
            var releases = await _db.GetAllEntitiesAsync<Release>();
            var randomRelease = releases.OrderBy(r => Guid.NewGuid()).FirstOrDefault();//new GUID as key, will be random

            if (randomRelease is null)
            {
                throw new Exception($"Error getting random release.");
            }
           
            var artists = await _db.GetAllEntitiesAsync<Artist>();
            var releaseArtistName = artists.Where(x => x.DiscogsArtistId == randomRelease.ArtistId).Select(x => x.Name).FirstOrDefault();


            var viewModel = new ReleaseViewModel
            {
                Artist = releaseArtistName ?? "Missing Artist",
                Year = randomRelease.Year,
                Title = randomRelease.Title,
                ResourceUrl = randomRelease.ResourceUrl,
                MasterUrl = randomRelease.MasterUrl,
                Genres = randomRelease.Genres,
                DateAdded = randomRelease.DateAdded
            };

            return viewModel;
        }

        public async Task<int> GetCollectionSize()
        {
            List<Release> releaseList;
            var releases = await _db.GetAllEntitiesAsync<Release>();
            releaseList = releases.ToList();

            if (!releaseList.Any())
            {
                releaseList = await _discogsApiService.GetCollectionFromDiscogsAndSaveAndReturn();
                releaseList.ToList();
            }         

            return releaseList.Count();
        }
    }
}
