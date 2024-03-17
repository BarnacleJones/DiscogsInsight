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
                var a = Constants.DatabasePath;//handy for debugging figuring out where the db is

                Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
                
                await Database.CreateTableAsync<Artist>();
                await Database.CreateTableAsync<Release>();
                await Database.CreateTableAsync<Track>();
                await Database.CreateTableAsync<MusicBrainzTags>();
                await Database.CreateTableAsync<MusicBrainzArtistToMusicBrainzTags>();
                await Database.CreateTableAsync<MusicBrainzArtistToMusicBrainzRelease>();
                await Database.CreateTableAsync<MusicBrainzReleaseToCoverImage>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb Init:{ex.Message} ");
                throw;
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
                throw;
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
                throw;
            }
        }

        public async Task Purge()
        {
            try
            {
                await Init();
                await Database.DeleteAllAsync<Artist>();
                await Database.DeleteAllAsync<Release>();
                //intentionally leaving other data. will save making api calls. will eventually move to another setting to clear that data too.
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb Purge:{ex.Message} ");
                throw;
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
                throw;
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
                throw;
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
                throw;
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
                throw;
            }
        }
    }
}

