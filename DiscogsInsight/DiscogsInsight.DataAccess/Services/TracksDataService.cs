using DiscogsInsight.Database.Entities;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.Database.Contract;

namespace DiscogsInsight.DataAccess.Services
{
    public class TracksDataService : ITracksDataService
    {
        private readonly ISQLiteAsyncConnection _db;

        public TracksDataService(ISQLiteAsyncConnection db)
        {
            _db = db;
        }

        public async Task<List<TrackGridModel>> GetAllTracksForGrid()
        {
            var tracksGridModelItemsQuery = $@"SELECT
                                               Id,
                                               DiscogsArtistId,
                                               DiscogsReleaseId,
                                               DiscogsMasterId,
                                               Title,
                                               Duration,
                                               MusicBrainzTrackLength,
                                               Position,
                                               Rating
                                               FROM Track";

            var data = await _db.QueryAsync<TrackGridModel>(tracksGridModelItemsQuery);

            return data;
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
