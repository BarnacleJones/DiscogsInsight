﻿using DiscogsInsight.DataModels;
using DiscogsInsight.Models;
using DiscogsInsight.ResponseModels;
using SQLite;

namespace DiscogsInsight
{
    public class DiscogsInsightDb


    {
        SQLiteAsyncConnection Database;

        public DiscogsInsightDb()
        {
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<Artist>();
            var result2 = await Database.CreateTableAsync<Release>();
        }

        public async Task<List<T>> GetAllEntitiesAsync<T>() where T : IDatabaseEntity, new()
        {
            await Init();
            return await Database.Table<T>().ToListAsync();
        }

        public async Task<bool> SaveDiscogsCollectionResponse(DiscogsCollectionResponse collectionResponse)
        {
            await SaveArtistsFromCollectionResponse(collectionResponse);
            await SaveReleasesWithArtistIds(collectionResponse);
            return true;
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
                throw new Exception(ex.Message);
            }
        }

        #region Private Methods
        private async Task SaveReleasesWithArtistIds(DiscogsCollectionResponse collectionResponse)
        {
            var artistsFromDb = await Database.Table<Artist>().ToListAsync();
            foreach (var release in collectionResponse.releases)
            {
                var artistIdForThisRelease = release.basic_information.artists.Select(x => x.id).FirstOrDefault();//only will save first artist for release, even though there may be many
                var artistIdFromDb = artistsFromDb.Where(x => x.DiscogsArtistId == artistIdForThisRelease).Select(x => x.DiscogsArtistId).FirstOrDefault();

                await Database.InsertAsync(new Release
                {
                    ArtistId = (artistIdForThisRelease == artistIdFromDb) ? artistIdForThisRelease : null,
                    DiscogsReleaseId = release.id,//this id is the same as basicinformation.id
                    DiscogsMasterId = release.basic_information.master_id,
                    Genres = string.Join(",", release.basic_information.genres),//intending not to save list, but string.join
                    MasterUrl = release.basic_information.master_url,
                    ResourceUrl = release.basic_information.resource_url,
                    Title = release.basic_information.title,
                    Year = release.basic_information.year,
                    DateAdded = release.date_added
                });

            }
        }

        private async Task SaveArtistsFromCollectionResponse(DiscogsCollectionResponse collectionResponse)
        {
            foreach (var release in collectionResponse.releases)
            {
                var artistsToSave = release.basic_information.artists.ToList();
                foreach (var artist in artistsToSave)
                {
                    var existingArtist = await Database.Table<Artist>().Where(x => x.DiscogsArtistId == artist.id).FirstOrDefaultAsync();
                    if (existingArtist == null)
                    {
                        await Database.InsertAsync(new Artist
                        {
                            DiscogsArtistId = artist.id,
                            Name = artist.name,
                            ResourceUrl = artist.resource_url
                        });
                    }
                }
            }
        }
        #endregion
    }
}

