using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using DiscogsInsight.ApiIntegration.MusicBrainzResponseModels;
using DiscogsInsight.ApiIntegration.Services;
using Microsoft.Extensions.Logging;
using Artist = DiscogsInsight.DataAccess.Entities.Artist;

namespace DiscogsInsight.DataAccess.Services
{
    public class ArtistDataService
    {
        private readonly DiscogsInsightDb _db;
        private readonly DiscogsApiService _discogsApiService;
        private readonly MusicBrainzApiService _musicBrainzApiService;
        private readonly ILogger<ArtistDataService> _logger;
        private readonly TagsDataService _tagsDataService;

        public ArtistDataService(DiscogsInsightDb db, 
            DiscogsApiService discogsApiService, 
            ILogger<ArtistDataService> logger, 
            MusicBrainzApiService musicBrainzApiService,
            TagsDataService tagsDataService)
        {
            _db = db;
            _discogsApiService = discogsApiService;
            _musicBrainzApiService = musicBrainzApiService;
            _logger = logger;
            _tagsDataService = tagsDataService;
        }

        public async Task<Artist?> GetArtistByDiscogsId(int discogsArtistId)
        {
            var artists = await _db.GetAllEntitiesAsync<Artist>();
            return artists.FirstOrDefault(x => x.DiscogsArtistId == discogsArtistId);
        }

        public async Task<List<Artist>> GetArtists()
        {
            return await _db.GetAllEntitiesAsync<Artist>();
        }

        public async Task<Artist?> GetArtist(int? discogsArtistId, bool fetchAndSaveApiData = true)
        {
            if (discogsArtistId == null) { return new Artist { Name = "No Artist Id Supplied" }; }//Todo: Eventually fix error a null displays throughout app
            bool saved = true;//Todo: I made this to just make sure api data is saved before getting artist

            if (fetchAndSaveApiData && discogsArtistId.HasValue)
            {
                saved = await GetAndSaveDiscogsArtistData((int)discogsArtistId);
                var musicBrainzId = await GetAndSaveInitialMusicBrainzArtistData((int)discogsArtistId);
                saved = !string.IsNullOrWhiteSpace(musicBrainzId);
                //saved = await GetArtistFromMusicBrainzApiUsingArtistId(musicBrainzId); unused at this point
            }

            if (saved)
            {
                return await GetArtistByDiscogsId((int)discogsArtistId);
            }

            return new Artist() { Name="Error saving artist"};

        }
        
        #region Private Discogs Artist Functions

        private async Task<bool> GetAndSaveDiscogsArtistData(int discogsArtistId)
        {
            var artistsTable = await _db.GetTable<Artist>();
            var existingArtist = await artistsTable.Where(x => x.DiscogsArtistId == discogsArtistId).FirstOrDefaultAsync();
            if (existingArtist == null)
            {
                //dont want to store the artist if not in db already
                //it hasnt been in the main discogs collection call
                //i dont know how would one get here...
                throw new Exception($"Unhandled exception: Artist {discogsArtistId} not in database not able to store info.");
            }
            if (string.IsNullOrEmpty(existingArtist.Profile))
            {
                var result = await _discogsApiService.GetArtistFromDiscogs(discogsArtistId);
                var saved = await SaveDiscogsArtistResponse(existingArtist, result);
                return saved;
            }
            return true;
        }

        private async Task<bool> SaveDiscogsArtistResponse(Artist existingArtist, DiscogsArtistResponse artistResponse)
        {
            try
            {               
                //additional properties from discogs artist call that arent on the main collection call
                existingArtist.Profile = artistResponse.profile;
                await _db.UpdateAsync(existingArtist);                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
                throw new Exception($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
            }
        }
        #endregion

        #region Private MusicBrainz Artist Functions

        private async Task<string> GetAndSaveInitialMusicBrainzArtistData(int discogsArtistId)
        {
            var artist = await GetArtistByDiscogsId(discogsArtistId);
            if (artist == null) { throw new Exception("Artist not found, error in GetAndSaveInitialMusicBrainzArtistData"); }

            if (artist.MusicBrainzArtistId == null)
            {
                var result = await _musicBrainzApiService.GetInitialArtistFromMusicBrainzApi(artist?.Name ?? "");
                return await SaveMusicBrainzInitialArtistResponseAndReturnMusicBrainzArtistId(result, discogsArtistId);
            }
            return artist.MusicBrainzArtistId;
        }

        private async Task<string> SaveMusicBrainzInitialArtistResponseAndReturnMusicBrainzArtistId(MusicBrainzInitialArtist artistResponse, int discogsArtistId)
        {
            try
            {
                var artistsTable = await _db.GetTable<Artist>();
                var existingArtist = await GetArtistByDiscogsId(discogsArtistId);
                if (existingArtist == null)
                {
                    //How would this happen...
                    throw new Exception($"You reached a how did this happen exception: Artist Id{discogsArtistId}) SaveMusicBrainzInitialArtistResponse");
                }
                else
                {
                    //**this makes an assumption that can cause bad data**
                    //Artists in the response is a list, there are similar named artists in the list
                    //It looks like the first in the list is closest match

                    //Todo: Add functionality that lets you review the artists list for the initial call to choose the proper one if data is wrong
                    var musicBrainsArtistId = artistResponse.Artists.Select(x => x.Id).FirstOrDefault();

                    existingArtist.MusicBrainzArtistId = musicBrainsArtistId;

                    var artistArea = artistResponse.Artists
                                                           .Where(x => x.Id == musicBrainsArtistId)
                                                           .Select(x => x.Area)
                                                           .FirstOrDefault();
                    var artistBeginArea = artistResponse.Artists
                                                           .Where(x => x.Id == musicBrainsArtistId)
                                                           .Select(x => x.BeginArea)
                                                           .FirstOrDefault();
                    
                    if (artistBeginArea != null)
                    {
                        var isACity = artistBeginArea.Type == "City";
                        var isACountry = artistBeginArea.Type == "Country";
                        if (isACity)
                        {
                            existingArtist.City = artistBeginArea.Name;
                        }
                        if (isACountry)
                        {
                            existingArtist.Country = artistBeginArea.Name;
                        }
                    }
                    
                    if (artistArea != null)
                    {
                        var isACity = artistArea.Type == "City";
                        var isACountry = artistArea.Type == "Country";
                        if (isACountry)
                        {
                            existingArtist.Country = artistArea.Name;
                        }
                        if (isACity)
                        {
                            existingArtist.City = artistArea.Name;
                        }
                    }

                    existingArtist.StartYear = artistResponse.Artists
                                                           .Where(x => x.Id == musicBrainsArtistId)
                                                           .Select(x => x.LifeSpan?.Begin)
                                                           .FirstOrDefault();

                    existingArtist.EndYear = artistResponse.Artists
                                                           .Where(x => x.Id == musicBrainsArtistId)
                                                           .Select(x => x.LifeSpan?.End)
                                                           .FirstOrDefault();

                    await _db.UpdateAsync(existingArtist);

                    //save tags
                    var success = await _tagsDataService.SaveTagsByMusicBrainzArtistId(artistResponse, musicBrainsArtistId);

                    return musicBrainsArtistId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
                throw new Exception($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
            }
        }

        #endregion
    }
}
