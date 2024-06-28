using DiscogsInsight.DataAccess.Models;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface IInsightsDataService
    {
        Task<List<CollectionStatisticData>> GetCollectionStatisticData();
        Task<List<ReleaseStatisticData>> GetReleaseStatisticData();
    }
}
