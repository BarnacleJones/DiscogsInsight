using Microsoft.Extensions.Logging;
using DiscogsInsight.DataAccess.Entities;
using DiscogsInsight.DataAccess.Contract;

namespace DiscogsInsight.DataAccess.Services
{
    public class TracksDataService : ITracksDataService
    {
        private readonly IDiscogsInsightDb _db;
        private readonly ILogger<TracksDataService> _logger;
        private IReleaseDataService _releaseDataService;

        public TracksDataService(IDiscogsInsightDb db, ILogger<TracksDataService> logger,IReleaseDataService releaseDataService)
        {
            _db = db;
            _logger = logger;
            _releaseDataService = releaseDataService;
        }


        public async Task<bool> SetRatingOnTrack(int? rating, int discogsReleaseId, string title)
        {
            try
            {
                var tracks = await GetAllTracks();
                if (tracks == null) { return true; }

                var track = tracks.Where(x => x.DiscogsReleaseId == discogsReleaseId)
                                  .Where(x => x.Title == title)
                                  .FirstOrDefault();
                if (track == null) { return true; }

                track.Rating = rating;
                await _db.SaveItemAsync(track);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public async Task<List<Track>> GetAllTracks()
        {
            var tracks = await _db.GetAllEntitiesAsListAsync<Track>();
            return tracks.ToList();
        }

        public async Task<List<Track>> GetTracksForRelease(int? discogsReleaseId)
        {
            try
            {
                if (discogsReleaseId == null) throw new Exception("No release id provided for getting tracklist");

                var tracks = await _db.GetAllEntitiesAsListAsync<Track>();

                var tracksForListRelease = tracks.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();

                var release = await _db.GetAllEntitiesAsListAsync<Release>();
                var thisRelease = release.ToList().FirstOrDefault(x => x.DiscogsReleaseId == discogsReleaseId);
            
                var updateReleaseWithReleaseResponse = !thisRelease.HasAllApiData;

                if (!tracksForListRelease.Any() || updateReleaseWithReleaseResponse)
                {
                    //updates all track info too - not using release info
                    var a = await _releaseDataService.GetReleaseAndImageAndRetrieveAllApiDataForRelease(discogsReleaseId);
                    tracks = await _db.GetAllEntitiesAsListAsync<Track>();

                    tracksForListRelease = tracks.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();
                }

                return tracksForListRelease;
            }
            catch (Exception)
            {
                throw;
            }
        }       
    }
}
