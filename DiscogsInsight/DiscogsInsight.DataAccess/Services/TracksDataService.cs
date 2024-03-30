using Microsoft.Extensions.Logging;
using DiscogsInsight.DataAccess.Entities;
using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using DiscogsInsight.ApiIntegration.Services;

namespace DiscogsInsight.DataAccess.Services
{
    public class TracksDataService
    {
        private readonly DiscogsInsightDb _db;
        private readonly ILogger<TracksDataService> _logger;
        private ReleaseDataService _releaseDataService;

        public TracksDataService(DiscogsInsightDb db, ILogger<TracksDataService> logger,ReleaseDataService releaseDataService)
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
            catch (Exception e)
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
                    var a = await _releaseDataService.GetRelease(discogsReleaseId);
                    tracks = await _db.GetAllEntitiesAsListAsync<Track>();

                    tracksForListRelease = tracks.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();
                }

                return tracksForListRelease;
            }
            catch (Exception ex)
            {
                throw;
            }
        }       
    }
}
