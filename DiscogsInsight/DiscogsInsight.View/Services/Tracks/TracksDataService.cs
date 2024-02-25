using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using DiscogsInsight.ApiIntegration.Services;
using DiscogsInsight.DataAccess;
using DiscogsInsight.DataAccess.Entities;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.View.Services.Tracks
{
    public class TracksViewService
    {
        private readonly DiscogsInsightDb _db;
        private readonly DiscogsApiService _discogsApiService;
        private readonly ILogger<TracksViewService> _logger;
        public TracksViewService(DiscogsApiService discogsApiService, DiscogsInsightDb db, ILogger<TracksViewService> logger) 
        {
            _discogsApiService = discogsApiService;
            _logger = logger;
            _db = db;
        }

        public async Task<List<Track>> GetTracksForRelease(int? discogsReleaseId)
        {
            if (discogsReleaseId == null) throw new Exception("No release id provided for getting tracklist");

            var tracks = await _db.GetAllEntitiesAsync<Track>();

            var trackList = tracks.ToList();

            var tracksForListRelease = trackList.Where(x => x.DiscogsReleaseId == discogsReleaseId);

            if (!tracksForListRelease.Any())
            {
                var discogsReleaseResponse = await _discogsApiService.GetReleaseFromDiscogs((int)discogsReleaseId);
                var success = await SaveTracksFromDiscogsReleaseResponse(discogsReleaseResponse);
                if (!success)
                {
                    throw new Exception("Error saving tracklist to database");
                }
                tracks = await _db.GetAllEntitiesAsync<Track>();
                trackList = tracks.ToList();
            }

            return trackList.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();
        }

        public async Task<bool> SaveTracksFromDiscogsReleaseResponse(DiscogsReleaseResponse releaseResponse)
        {           
            //save tracklists, make tracklist entity
            try
            {
                var releaseTable = await _db.GetTable<Release>();
                var existingRelease = releaseTable.Where(x => x.DiscogsReleaseId == releaseResponse.id).FirstOrDefaultAsync().Result;
                var tracksTable = await _db.GetTable<Track>();
                var existingTracks = tracksTable.Where(x => x.DiscogsReleaseId == releaseResponse.id).ToListAsync().Result;
                if (existingRelease == null)
                    //at this stage, dont want to store the release info if not in db already
                    throw new Exception($"Unhandled exception: Release {releaseResponse.id} not in database not able to store info.");

                //update existing release entity with additional properties

                existingRelease.ReleaseCountry = releaseResponse.country;
                await _db.UpdateAsync(existingRelease);


                if (!existingTracks.Any() && releaseResponse.tracklist != null)
                {
                    foreach (var track in releaseResponse.tracklist)
                    {
                        await _db.SaveItemAsync<Track>(new Track
                        {
                            DiscogsArtistId = existingRelease.DiscogsArtistId,
                            DiscogsMasterId = existingRelease.DiscogsMasterId,
                            DiscogsReleaseId = releaseResponse.id,
                            Duration = track.duration,
                            Title = track.title,
                            Position = track.position
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
                throw new Exception($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
            }
            return true;            
        }              
    }    
}
