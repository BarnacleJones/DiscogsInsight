using DiscogsInsight.DataModels;
using DiscogsInsight.ViewModels;

namespace DiscogsInsight.Services
{
    public class ArtistService
    {
        private readonly CollectionDataService _db;
        private readonly DiscogsApiService _discogsApiService;

        public ArtistService(CollectionDataService db, DiscogsApiService discogsApiService)
        {
            _db = db;
            _discogsApiService = discogsApiService;
        }

        public async Task<List<ArtistViewModel>> GetArtists()
        {
            List<Release> releaseList;
            var releases = await _db.GetAllEntitiesAsync<Release>();
            releaseList = releases.ToList();

            if (!releaseList.Any())
            {
                //seed db if nothing there yet
                releaseList = await _discogsApiService.GetCollectionFromDiscogsAndSaveAndReturn();
            }
            var artists = await _db.GetAllEntitiesAsync<Artist>();

            return artists.Select(x => new ArtistViewModel
            {
                Artist = x.Name,
                DiscogsArtistId = x.DiscogsArtistId

            }).ToList();
        }

        public async Task<ArtistViewModel> GetArtist(int? discogsArtistId)
        {
            if (discogsArtistId == null)
                throw new Exception($"Missing artist info");
                

            var artists = await _db.GetAllEntitiesAsync<Artist>();
            var artist = artists.FirstOrDefault(x => x.DiscogsArtistId == discogsArtistId);

            if (artist == null || artist?.Profile == null)//artist.Profile is retrieved on a different API call which is only called in this scenario
            {
                var result = await _discogsApiService.GetArtistFromDiscogsAndSave(discogsArtistId.Value);

                var newArtists = await _db.GetAllEntitiesAsync<Artist>();
                artist = newArtists.FirstOrDefault(x => x.DiscogsArtistId == discogsArtistId);
            }

            return new ArtistViewModel
            {
                Artist = artist.Name,
                ArtistDescription = artist.Profile,
                DiscogsArtistId = artist.DiscogsArtistId
            };           
            
        }
    }
}
