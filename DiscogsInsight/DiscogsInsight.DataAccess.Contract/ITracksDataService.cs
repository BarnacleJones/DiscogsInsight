namespace DiscogsInsight.DataAccess.Contract
{
    public interface ITracksDataService
    {
        Task<bool> SetRatingOnTrack(int? rating, int discogsReleaseId, string title);
    }
}
