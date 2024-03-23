using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using DiscogsInsight.ApiIntegration.Services;
using DiscogsInsight.DataAccess;
using DiscogsInsight.DataAccess.Entities;
using DiscogsInsight.ViewModels.Results;
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

        public async Task<ViewResult<List<Track>>> GetTracksForRelease(int? discogsReleaseId)
        {
            try
            {
                if (discogsReleaseId == null) throw new Exception("No release id provided for getting tracklist");

                var tracks = await _db.GetAllEntitiesAsync<Track>();

                var trackList = tracks.ToList();

                var tracksForListRelease = trackList.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();

                if (!tracksForListRelease.Any())
                {
                    var discogsReleaseResponse = await _discogsApiService.GetReleaseFromDiscogs((int)discogsReleaseId);
                    var success = await SaveTracksAndAdditionalInformationFromDiscogsReleaseResponse(discogsReleaseResponse);
                    if (!success.Success)
                    {
                        throw new Exception("Error saving tracklist to database");
                    }
                    tracks = await _db.GetAllEntitiesAsync<Track>();
                    trackList = tracks.ToList();
                }

                return new ViewResult<List<Track>>
                {
                    Data = trackList.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList(),
                    ErrorMessage = "",
                    Success = true
                };

            }
            catch (Exception ex)
            {
                return new ViewResult<List<Track>>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }

        public async Task<ViewResult<bool>> SaveTracksAndAdditionalInformationFromDiscogsReleaseResponse(DiscogsReleaseResponse releaseResponse)
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

                return new ViewResult<bool> { Success = true, ErrorMessage = "" };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");

                return new ViewResult<bool>
                {
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }           
        }              
    }    
}
