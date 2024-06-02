using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface ITracksDataService
    {
        Task<bool> SetRatingOnTrack(int? rating, int discogsReleaseId, string title);
        Task<List<Track>> GetAllTracks();
        Task<List<Track>> GetTracksForRelease(int? discogsReleaseId);
    }
}
