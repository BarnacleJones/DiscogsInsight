using DiscogsInsight.Database.Contract;
using DiscogsInsight.Database.Entities;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.Database.Services
{
    public class DiscogsInsightDb : IDiscogsInsightDb
    {
        private readonly ILogger<DiscogsInsightDb> _logger;
        private readonly ISQLiteAsyncConnection _database;

        public DiscogsInsightDb(ILogger<DiscogsInsightDb> logger, ISQLiteAsyncConnection database)
        {
            _logger = logger;
            _database = database;
        }
             

        public async Task<List<T>> GetAllEntitiesAsListAsync<T>() where T : new()
        {
            try
            {
                return await _database.Table<T>();
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
                if (item.Id != 0)
                    return await _database.UpdateAsync(item);

                return await _database.InsertAsync(item);
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
                await _database.DeleteAllAsync<Artist>();
                await _database.DeleteAllAsync<Release>();
                //intentionally leaving other data. Use PurgeEntireDb for the other
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb Purge:{ex.Message} ");
                throw;
            }
        }

        public async Task PurgeEntireDb()
        {
            try
            {
                await _database.DeleteAllAsync<Artist>();
                await _database.DeleteAllAsync<DiscogsGenreTags>();
                await _database.DeleteAllAsync<DiscogsGenreTagToDiscogsRelease>();
                await _database.DeleteAllAsync<MusicBrainzArtistToMusicBrainzRelease>();
                await _database.DeleteAllAsync<MusicBrainzArtistToMusicBrainzTags>();
                await _database.DeleteAllAsync<MusicBrainzReleaseToCoverImage>();
                await _database.DeleteAllAsync<MusicBrainzTags>();
                await _database.DeleteAllAsync<Release>();
                await _database.DeleteAllAsync<Track>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb Purge all database data:{ex.Message} ");
                throw;
            }
        }

        public async Task<int> DeleteAsync<T>(T entity) where T : IDatabaseEntity, new()
        {
            try
            {
                var entityToDelete = await _database.GetAsync<T>(pk: entity.Id);
                var a = await _database.DeleteAsync(entityToDelete);
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
                var a = await _database.InsertAsync(entity);
                
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
                var a = await _database.UpdateAsync(entity);
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

