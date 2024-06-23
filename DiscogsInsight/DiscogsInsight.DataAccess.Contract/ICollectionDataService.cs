using DiscogsInsight.Database.Contract;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface ICollectionDataService
    {
        Task<List<SimpleReleaseData>> GetSimpleReleaseDataForWholeCollection();
        Task<List<SimpleReleaseData>> GetSimpleReleaseDataForCollectionDataWithoutAllApiData();
        Task<bool> UpdateCollectionFromDiscogs();
        Task<bool> CheckCollectionIsSeededOrSeed();
    }
}
