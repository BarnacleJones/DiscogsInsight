namespace DiscogsInsight.Database.Contract
{
    public interface IDiscogsInsightDb
    {
        Task<List<T>> GetAllEntitiesAsListAsync<T>() where T : new();
        Task Purge();
        Task PurgeEntireDb();
        Task<int> DeleteAsync<T>(T entity) where T : IDatabaseEntity, new();
        Task<int> InsertAsync<T>(T entity) where T : IDatabaseEntity, new();
        Task<int> UpdateAsync<T>(T entity) where T : IDatabaseEntity, new();

    }
}
