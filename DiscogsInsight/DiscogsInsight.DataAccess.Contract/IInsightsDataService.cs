using DiscogsInsight.DataAccess.Models;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface IInsightsDataService
    {
        //Task<TracksInsightDataModel> GetTrackInsightData(); //No ideas yet
        Task<CollectionStatisticData> GetCollectionStatisticData();
        Task<List<ReleaseStatisticData>> GetReleaseStatisticData();
    }
}
