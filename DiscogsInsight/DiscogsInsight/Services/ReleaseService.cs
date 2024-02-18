using DiscogsInsight.DataModels;
using DiscogsInsight.ViewModels;

namespace DiscogsInsight.Services
{
    public class ReleaseService
    {
        private readonly CollectionDataService _db;
        private readonly DiscogsApiService _discogsApiService;

        public ReleaseService(CollectionDataService db, DiscogsApiService discogsApiService)
        {
            _db = db;
            _discogsApiService = discogsApiService;
        }



        public async Task<ReleaseViewModel> GetRelease(int? discogsReleaseId)
        {
            if (discogsReleaseId == null)
                throw new Exception($"Missing release info");

            var releaseTrackList = new List<TrackViewModel>();
            var releases = await _db.GetAllEntitiesAsync<Release>();
            var release = releases.FirstOrDefault(x => x.DiscogsReleaseId == discogsReleaseId);

            var artists = await _db.GetAllEntitiesAsync<Artist>();
            var artist = artists.FirstOrDefault(x => x.DiscogsArtistId == release.DiscogsArtistId);

            var tracks = await _db.GetAllEntitiesAsync<Track>();
            var releaseTracks = tracks.Where(x => x.DiscogsReleaseId == discogsReleaseId);

            if (release == null || !releaseTracks.Any())
            {
                var result = await _discogsApiService.GetReleaseFromDiscogsAndSave(discogsReleaseId.Value);

                var newRelease = await _db.GetAllEntitiesAsync<Release>();
                release = newRelease.FirstOrDefault(x => x.DiscogsReleaseId == discogsReleaseId);

                tracks = await _db.GetAllEntitiesAsync<Track>();
            }

            releaseTrackList = tracks.Where(x => x.DiscogsReleaseId == discogsReleaseId).Select(x => new TrackViewModel 
            {
                Title = x.Title,
                Duration = x.Duration,
                Position = x.Position
            })
            .ToList();
            return new ReleaseViewModel
            {
                Artist = artist.Name,
                Title = release.Title,
                DateAdded = release.DateAdded,
                Genres = release.Genres,
                Year = release.Year,
                Tracks = releaseTrackList,
                ReleaseCountry = release.ReleaseCountry
            };           
            
        }
    }
}
