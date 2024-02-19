using DiscogsInsight.DataModels;
using DiscogsInsight.ViewModels;

namespace DiscogsInsight.Services
{
    public class CollectionService
    {
        private readonly CollectionDataService _db;
        private readonly DiscogsApiService _discogsApiService;

        public CollectionService(CollectionDataService db, DiscogsApiService discogsApiService)
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
                Artist = artistIdsList.Where(y => y.DiscogsArtistId == x.DiscogsArtistId).Select(x => x.Name).FirstOrDefault() ?? "ERROR",//Todo: make this better
                DiscogsArtistId = x.DiscogsArtistId,
                DiscogsReleaseId = x.DiscogsReleaseId,
                Year = x.Year,
                Title = x.Title,
                Genres = x.Genres,
                DateAdded = x.DateAdded
            }).ToList();

            return new DiscogsCollection { Releases = viewModel };
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
