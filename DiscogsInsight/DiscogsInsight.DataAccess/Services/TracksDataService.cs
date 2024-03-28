using Microsoft.Extensions.Logging;
using DiscogsInsight.DataAccess.Entities;
using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using DiscogsInsight.ApiIntegration.Services;

namespace DiscogsInsight.DataAccess.Services
{
    public class TracksDataService
    {
        private readonly DiscogsInsightDb _db;
        private readonly ILogger<ArtistDataService> _logger;
        private DiscogsApiService _discogsApiService;

        public TracksDataService(DiscogsInsightDb db,
            ILogger<ArtistDataService> logger, DiscogsApiService discogsApiService)
        {
            _db = db;
            _logger = logger;
            _discogsApiService = discogsApiService;
        }

        public async Task<List<Track>> GetAllTracks()
        {
            var tracks = await _db.GetAllEntitiesAsync<Track>();
            return tracks.ToList();
        }

        public async Task<List<Track>> GetTracksForRelease(int? discogsReleaseId)
        {
            try
            {
                if (discogsReleaseId == null) throw new Exception("No release id provided for getting tracklist");

                var tracks = await _db.GetAllEntitiesAsync<Track>();

                var trackList = tracks.ToList();

                var tracksForListRelease = trackList.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();

                //there is a better way to do this, especially not in the view service
                //but for now this is the place I will update the whatever is the latest 
                //property added to the release entity, to help determine if api needs to be hit again
                var release = await _db.GetAllEntitiesAsync<Release>();
                var thisRelease = release.ToList().FirstOrDefault(x => x.DiscogsReleaseId == discogsReleaseId);
                //latest added is release notes
                var updateReleaseWithReleaseResponse = thisRelease.ReleaseNotes == null;

                if (!tracksForListRelease.Any() || updateReleaseWithReleaseResponse)
                {
                    //updateReleaseWithReleaseResponse is option to save additional information again for data that is missing
                    //such as when entities existed and i then came and added url to the entity. wasnt getting updated because tracks were retrieved.
                    //dont want to purge track info when its just to get these few properties
                    var discogsReleaseResponse = await _discogsApiService.GetReleaseFromDiscogs((int)discogsReleaseId);
                    var success = await SaveTracksAndAdditionalInformationFromDiscogsReleaseResponse(discogsReleaseResponse);
                    if (!success)
                    {
                        throw new Exception("Error saving tracklist to database");
                    }
                    tracks = await _db.GetAllEntitiesAsync<Track>();
                    trackList = tracks.ToList();
                }

                return trackList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<bool> SaveTracksAndAdditionalInformationFromDiscogsReleaseResponse(DiscogsReleaseResponse releaseResponse)
        {
            try
            {
                var releaseTable = await _db.GetTable<Release>();
                var existingRelease = await releaseTable.Where(x => x.DiscogsReleaseId == releaseResponse.id).FirstOrDefaultAsync();
                var tracksTable = await _db.GetTable<Track>();
                var existingTracks = await tracksTable.Where(x => x.DiscogsReleaseId == releaseResponse.id).ToListAsync();
                if (existingRelease == null)
                    //at this stage, dont want to store the release info if not in db already
                    throw new Exception($"Unhandled exception: Release {releaseResponse.id} not in database not able to store info.");

                //update existing release entity with additional properties
                existingRelease.ReleaseCountry = releaseResponse.country;
                existingRelease.ReleaseNotes = releaseResponse.notes;
                existingRelease.DiscogsReleaseUrl = releaseResponse.uri;
                await _db.UpdateAsync(existingRelease);

                //save the tracks
                if (existingTracks != null && !existingTracks.Any() && releaseResponse.tracklist != null)
                {
                    foreach (var track in releaseResponse.tracklist)
                    {
                        await _db.SaveItemAsync(new Track
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
                //save the genre tags
                var discogsGenreTagsToDiscogsReleaseTable = await _db.GetTable<DiscogsGenreTagToDiscogsRelease>();
                var discogsGenreTags = await _db.GetTable<DiscogsGenreTags>();

                var existingGenreTags = await tracksTable.Where(x => x.DiscogsReleaseId == releaseResponse.id).ToListAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
                throw;
            }
        }

    }
}
