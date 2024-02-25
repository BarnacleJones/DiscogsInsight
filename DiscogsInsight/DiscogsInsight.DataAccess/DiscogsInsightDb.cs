using DiscogsInsight.DataAccess.Entities;
using DiscogsInsight.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;
using SQLite;

namespace DiscogsInsight.DataAccess
{

    public class DiscogsInsightDb
    {

        private readonly ILogger<DiscogsInsightDb> _logger;
        SQLiteAsyncConnection? Database;

        public DiscogsInsightDb(ILogger<DiscogsInsightDb> logger)
        {
            _logger = logger;
        }

        async Task Init()
        {
            try
            {
                if (Database is not null)
                    return;

                Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
                //todo: result needed still?
                var result = await Database.CreateTableAsync<Artist>();
                var result2 = await Database.CreateTableAsync<Release>();
                var result3 = await Database.CreateTableAsync<Track>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb Init:{ex.Message} ");
                throw new Exception($"Exception at Db Init:{ex.Message} ");
            }
        }

        public async Task<List<T>> GetAllEntitiesAsync<T>() where T : IDatabaseEntity, new()
        {
            try
            {
                await Init();
                return await Database.Table<T>().ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb GetAllEntitiesAsync:{ex.Message} ");
                throw new Exception($"Exception at GetAllEntitiesAsync:{ex.Message} ");
            }
        }

        public async Task<int> SaveItemAsync<T>(T item) where T : IDatabaseEntity
        {
            try
            {
                await Init();
                if (item.Id != 0)
                    return await Database.UpdateAsync(item);

                return await Database.InsertAsync(item);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb SaveItemAsync:{ex.Message} ");
                throw new Exception($"Exception at SaveItemAsync:{ex.Message} ");
            }
        }

        public async Task Purge()
        {
            try
            {
                await Init();
                await Database.DeleteAllAsync<Artist>();
                await Database.DeleteAllAsync<Release>();
                await Database.DeleteAllAsync<Track>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb Purge:{ex.Message} ");
                throw new Exception($"Exception at Purge:{ex.Message} ");
            }
        }

        public async Task<AsyncTableQuery<T>> GetTable<T>() where T : IDatabaseEntity, new()
        {
            try
            {
                await Init();
                return await Task.FromResult(Database.Table<T>());

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb GetTable:{ex.Message} ");
                throw new Exception($"Exception at GetTable:{ex.Message} ");
            }
        }

        public async Task<int> DeleteAsync<T>(T entity) where T : IDatabaseEntity, new()
        {
            try
            {
                await Init();
                var a = await Database.DeleteAsync<T>(entity.Id);
                return a;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb DeleteAsync:{ex.Message} ");
                throw new Exception($"Exception at DeleteAsync:{ex.Message} ");
            }
        }

        public async Task<int> InsertAsync<T>(T entity) where T : IDatabaseEntity, new()
        {
            try
            {
                await Init();
                var a = await Database.InsertAsync(entity);
                return a;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb InsertAsync:{ex.Message} ");
                throw new Exception($"Exception at InsertAsync:{ex.Message} ");
            }
        }
        public async Task<int> UpdateAsync<T>(T entity) where T : IDatabaseEntity, new()
        {
            try
            {
                await Init();
                var a = await Database.UpdateAsync(entity);
                return a;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb UpdateAsync:{ex.Message} ");
                throw new Exception($"Exception at UpdateAsync:{ex.Message} ");
            }
        }
    }
}

