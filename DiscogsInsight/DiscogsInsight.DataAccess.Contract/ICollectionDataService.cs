namespace DiscogsInsight.DataAccess.Contract
{
    public interface ICollectionDataService
    {
        Task<bool> UpdateCollectionFromDiscogs();
        Task<bool> CheckCollectionIsSeededOrSeed();
    }
}
