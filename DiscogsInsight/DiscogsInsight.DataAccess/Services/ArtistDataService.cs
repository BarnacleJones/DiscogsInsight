using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using DiscogsInsight.ApiIntegration.Services;
using DiscogsInsight.DataAccess.Entities;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.DataAccess.Services
{
    public class ArtistDataService
    {
        private readonly DiscogsInsightDb _db;
        private readonly DiscogsApiService _discogsApiService;
        private readonly ILogger<ArtistDataService> _logger;

        public ArtistDataService(DiscogsInsightDb db, DiscogsApiService discogsApiService, ILogger<ArtistDataService> logger)
        {
            _db = db;
            _discogsApiService = discogsApiService;
            _logger = logger;
        }

        
        public async Task<List<Artist>> GetArtists()
        {
            return await _db.GetAllEntitiesAsync<Artist>();
        }

        public async Task<Artist> GetArtist(int? discogsArtistId)
        {
            if (discogsArtistId == null)
                throw new Exception($"Missing artist info");

            var artists = await _db.GetAllEntitiesAsync<Artist>();
            var artist = artists.FirstOrDefault(x => x.DiscogsArtistId == discogsArtistId);

            if (artist == null || artist?.Profile == null)//artist.Profile is retrieved on a different API call which is only called in this scenario
            {
                var result = await _discogsApiService.GetArtistFromDiscogs(discogsArtistId.Value);

                var saved = await SaveDiscogsArtistResponse(result);

                if (!saved)
                {
                    throw new Exception("Unhandled exception: Artist from discogs API not saved");
                }
                var newArtists = await _db.GetAllEntitiesAsync<Artist>();
                artist = newArtists.FirstOrDefault(x => x.DiscogsArtistId == discogsArtistId);
            }
            return artist ?? new Artist();

        }

        private async Task<bool> SaveDiscogsArtistResponse(DiscogsArtistResponse releaseResponse)
        {
            try
            {
                var artistsTable = await _db.GetTable<Artist>();
                var existingArtist = artistsTable.Where(x => x.DiscogsArtistId == releaseResponse.id).FirstOrDefaultAsync().Result;
                if (existingArtist == null)
                {
                    //dont want to store the artist if not in db already
                    throw new Exception($"Unhandled exception: Artist {releaseResponse.id} not in database not able to store info.");
                }
                else
                {
                    existingArtist.Profile = releaseResponse.profile;
                    await _db.UpdateAsync(existingArtist);
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
