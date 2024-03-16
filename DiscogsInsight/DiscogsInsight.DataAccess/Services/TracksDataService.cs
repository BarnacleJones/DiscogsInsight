using Microsoft.Extensions.Logging;
using DiscogsInsight.DataAccess.Entities;

namespace DiscogsInsight.DataAccess.Services
{
    public class TracksDataService
    {
        private readonly DiscogsInsightDb _db;
        private readonly ILogger<ArtistDataService> _logger;

        public TracksDataService(DiscogsInsightDb db,
            ILogger<ArtistDataService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<Track>> GetAllTracks()
        {
            var tracks = await _db.GetAllEntitiesAsync<Track>();
            return tracks.ToList();
        }
               
    }
}
