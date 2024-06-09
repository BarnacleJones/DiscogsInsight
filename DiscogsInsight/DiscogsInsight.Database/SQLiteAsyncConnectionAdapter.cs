﻿using SQLite;
using Microsoft.Extensions.Logging;
using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.Database
{
    public class SQLiteAsyncConnectionAdapter : Contract.ISQLiteAsyncConnection
    {
        SQLiteAsyncConnection _connection;
        ILogger<SQLiteAsyncConnectionAdapter> _logger;

        public SQLiteAsyncConnectionAdapter(ILogger<SQLiteAsyncConnectionAdapter> logger)
        {
            _logger = logger;
            _connection = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            _ = InitializeAsync();
        }

        public async Task Purge()
        {
            try
            {
                await _connection.DeleteAllAsync<Artist>();
                await _connection.DeleteAllAsync<Release>();
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
                await _connection.DeleteAllAsync<Artist>();
                await _connection.DeleteAllAsync<DiscogsGenreTags>();
                await _connection.DeleteAllAsync<DiscogsGenreTagToDiscogsRelease>();
                await _connection.DeleteAllAsync<MusicBrainzArtistToMusicBrainzRelease>();
                await _connection.DeleteAllAsync<MusicBrainzArtistToMusicBrainzTags>();
                await _connection.DeleteAllAsync<MusicBrainzReleaseToCoverImage>();
                await _connection.DeleteAllAsync<MusicBrainzTags>();
                await _connection.DeleteAllAsync<Release>();
                await _connection.DeleteAllAsync<Track>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb Purge all database data:{ex.Message} ");
                throw;
            }
        }
        private async Task InitializeAsync()
        {
            try
            {
                var a = Constants.DatabasePath;//handy for debugging figuring out where the db is

                await _connection.CreateTableAsync<Artist>();
                await _connection.CreateTableAsync<Release>();
                await _connection.CreateTableAsync<Track>();
                await _connection.CreateTableAsync<MusicBrainzTags>();
                await _connection.CreateTableAsync<MusicBrainzArtistToMusicBrainzTags>();
                await _connection.CreateTableAsync<MusicBrainzArtistToMusicBrainzRelease>();
                await _connection.CreateTableAsync<MusicBrainzReleaseToCoverImage>();
                await _connection.CreateTableAsync<DiscogsGenreTagToDiscogsRelease>();
                await _connection.CreateTableAsync<DiscogsGenreTags>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at DiscogsInsightDb Init:{ex.Message} ");
                throw;
            }
        }

        public Task CreateTableAsync<T>() where T : new()
        {
            return _connection.CreateTableAsync<T>();
        }

        public Task<int> DeleteAllAsync<T>() where T : new()
        {
            return _connection.DeleteAllAsync<T>();
        }

        public Task<int> DeleteAsync(object obj)
        {
            return _connection.DeleteAsync(obj);
        }

        public Task<int> ExecuteAsync(string query, params object[] args)
        {
            throw new NotImplementedException();
        }

        public Task<T> FindAsync<T>(object pk) where T : new()
        {
            return _connection.FindAsync<T>(pk);
        }

        public Task<T> GetAsync<T>(object pk) where T : new()
        {
            return _connection.GetAsync<T>(pk);
        }

        public Task<int> InsertAsync(object obj)
        {
            return _connection.InsertAsync(obj);
        }

        public Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : new()
        {
            return _connection.QueryAsync<T>(query, args);
        }

        public AsyncTableQuery<T> Table<T>() where T : new()
        {
            return _connection.Table<T>();
        }

        public Task<int> UpdateAsync(object obj)
        {
            return _connection.UpdateAsync(obj);
        }
    }
}
