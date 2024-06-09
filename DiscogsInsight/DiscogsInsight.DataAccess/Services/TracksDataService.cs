using Microsoft.Extensions.Logging;
using DiscogsInsight.Database.Entities;
using DiscogsInsight.DataAccess.Contract;

namespace DiscogsInsight.DataAccess.Services
{
    public class TracksDataService : ITracksDataService
    {
        private readonly Database.Contract.ISQLiteAsyncConnection _db;
        private readonly ILogger<TracksDataService> _logger;

        public TracksDataService(Database.Contract.ISQLiteAsyncConnection db, ILogger<TracksDataService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<Track>> GetAllTracks()
        {
            return await _db.Table<Track>().ToListAsync();
        }

        public async Task<bool> SetRatingOnTrack(int? rating, int discogsReleaseId, string title)
        {
            var track = await _db.Table<Track>().Where(x => x.DiscogsReleaseId == discogsReleaseId).Where(x => x.Title == title).FirstOrDefaultAsync();
                
            if (track == null) { return true; }

            track.Rating = rating;
            await _db.UpdateAsync(track);

            return true;           
            
        }
    }
}
