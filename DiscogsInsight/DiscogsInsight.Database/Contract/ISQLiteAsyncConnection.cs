using SQLite;

namespace DiscogsInsight.Database.Contract
{ 
    public interface ISQLiteAsyncConnection
    {
        Task CreateTableAsync<T>() where T : new();
        AsyncTableQuery<T> Table<T>() where T : new();
        Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : new();
        Task<T> FindAsync<T>(object pk) where T : new();
        Task<T> GetAsync<T>(object pk) where T : new();
        Task<int> InsertAsync(object obj);
        Task<int> UpdateAsync(object obj);
        Task<int> DeleteAsync(object obj);
        Task<int> ExecuteAsync(string query, params object[] args);
        Task<int> DeleteAllAsync<T>() where T : new();
    }
}