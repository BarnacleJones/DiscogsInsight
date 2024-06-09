using DiscogsInsight.DataAccess.Models;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface ITracksDataService
    {
        Task<bool> SetRatingOnTrack(int? rating, int discogsReleaseId, string title);
        Task<List<TrackGridModel>> GetAllTracksForGrid();
    }
}
