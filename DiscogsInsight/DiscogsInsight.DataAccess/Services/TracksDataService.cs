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
        private DiscogsApiService _discogsApiService;
        private DiscogsGenresAndTagsDataService _discogsGenresAndTagsDataService;

        public TracksDataService(DiscogsInsightDb db,
            ILogger<TracksDataService> logger, DiscogsApiService discogsApiService,
             DiscogsGenresAndTagsDataService discogsGenresAndTagsDataService)
        {
            _db = db;
            _logger = logger;
            _discogsApiService = discogsApiService;
            _discogsGenresAndTagsDataService = discogsGenresAndTagsDataService;
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

                var trackList = tracks.ToList();

                var tracksForListRelease = trackList.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();

                var release = await _db.GetAllEntitiesAsListAsync<Release>();
                var thisRelease = release.ToList().FirstOrDefault(x => x.DiscogsReleaseId == discogsReleaseId);
            
                var updateReleaseWithReleaseResponse = !thisRelease.HasAllApiData;

                if (!tracksForListRelease.Any() || updateReleaseWithReleaseResponse)
                {
                    var discogsReleaseResponse = await _discogsApiService.GetReleaseFromDiscogs((int)discogsReleaseId);
                    var success = await SaveTracksAndAdditionalInformationFromDiscogsReleaseResponse(discogsReleaseResponse);
                    if (!success)
                    {
                        throw new Exception("Error saving tracklist to database");
                    }
                    tracks = await _db.GetAllEntitiesAsListAsync<Track>();
                    trackList = tracks.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();
                    thisRelease.HasAllApiData = true;
                    await _db.UpdateAsync(thisRelease);
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
                if (existingRelease == null || existingRelease.DiscogsArtistId == null || existingRelease.DiscogsReleaseId == null)
                    //at this stage, dont want to store the release info if not in db already
                    throw new Exception($"Unhandled exception: Release {releaseResponse.id} not in database not able to store info.");

                await UpdateAdditionalReleaseProperties(releaseResponse, existingRelease);//todo: this could be moved to release data service

                await SaveTracksFromDiscogsReleaseResponse(releaseResponse, existingRelease, existingTracks);
                
                //save genres (styles) from release
                var success = await _discogsGenresAndTagsDataService.SaveStylesFromDiscogsRelease(releaseResponse, existingRelease.DiscogsReleaseId.Value, existingRelease.DiscogsArtistId.Value);

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
                throw;
            }
        }

        private async Task SaveTracksFromDiscogsReleaseResponse(DiscogsReleaseResponse releaseResponse, Release existingRelease, List<Track> existingTracks)
        {
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
        }

        private async Task UpdateAdditionalReleaseProperties(DiscogsReleaseResponse releaseResponse, Release existingRelease)
        {
            //update existing release entity with additional properties
            existingRelease.ReleaseCountry = releaseResponse.country;
            existingRelease.ReleaseNotes = releaseResponse.notes;
            existingRelease.DiscogsReleaseUrl = releaseResponse.uri;
            await _db.UpdateAsync(existingRelease);
        }
    }
}
