using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using DiscogsInsight.ApiIntegration.Services;
using DiscogsInsight.DataAccess.Entities;
using DiscogsInsight.DataAccess.Interfaces;
using DiscogsInsight.DataAccess.Models;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.DataAccess.Services
{
    public class CollectionDataService
    {
        private readonly DiscogsInsightDb _db;
        private readonly DiscogsApiService _discogsApiService;
        private readonly ILogger<CollectionDataService> _logger;

        public CollectionDataService(DiscogsInsightDb db, DiscogsApiService discogsApiService, ILogger<CollectionDataService> logger)
        {
            _db = db;
            _discogsApiService = discogsApiService;
            _logger = logger;
        }

        #region Public Methods
        public async Task<List<DiscogsArtistIdAndName>> GetArtistsIdsAndNames()
        {
            var artistList = await GetCollectionEntityAsList<Artist>();

            return artistList.Select(x => 
            new DiscogsArtistIdAndName 
            {
                DiscogsArtistId = x.DiscogsArtistId,
                Name = x.Name 
            }).ToList();            
        }

        public async Task<List<Release>> GetReleases()
        {
            return await GetCollectionEntityAsList<Release>();
        }

        #endregion

        #region Private

        //Use this method to get item from database and get collection from API if doesnt exist
        private async Task<List<T>> GetCollectionEntityAsList<T>() where T : IDatabaseEntity, new()
        {
            List<T> entityList;
            var entities = await _db.GetAllEntitiesAsync<T>();
            entityList = entities.ToList();

            if (!entityList.Any())
            {
                await CollectionSavedOrUpdatedFromDiscogs();
                var newEntity = await _db.GetAllEntitiesAsync<T>();
                entityList = newEntity.ToList();
            }
            return entityList;
        }

        public async Task PurgeEntireCollection()
        {
            await _db.Purge();
        }
        public async Task<bool> CollectionSavedOrUpdatedFromDiscogs()
        {
            try
            {
                var data = await _discogsApiService.GetCollectionFromDiscogsApi();

                return await SaveDiscogsCollectionResponse(data);               
            }
            catch
            {
                throw;
            }
        }
        private async Task<bool> SaveDiscogsCollectionResponse(DiscogsCollectionResponse collectionResponse)
        {
            await SaveArtistsFromCollectionResponse(collectionResponse);
            await SaveReleasesWithArtistIds(collectionResponse);
            //Todo: These would be a good behind a setting
            await RemoveReleasesNoLongerInCollection(collectionResponse);
            await RemoveArtistsNoLongerInCollection(collectionResponse);
            return true;
        }
        private async Task SaveArtistsFromCollectionResponse(DiscogsCollectionResponse collectionResponse)
        {
            try
            {
                foreach (var release in collectionResponse.releases)
                {
                    var artistsToSave = release.basic_information?.artists?.ToList();
                    foreach (var artist in artistsToSave)
                    {
                        var artistsTable = await _db.GetTable<Artist>();
                        var existingArtist = artistsTable.Where(x => x.DiscogsArtistId == artist.id).FirstOrDefaultAsync();
                        if (await existingArtist == null)
                        {
                            await _db.InsertAsync(new Artist
                            {
                                DiscogsArtistId = artist.id,
                                Name = artist.name,
                            });
                        }
                        else
                        {
                            await _db.UpdateAsync(new Artist
                            {
                                Id = existingArtist.Id,
                                DiscogsArtistId = artist.id,
                                Name = artist.name
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
                throw;
            }
        }
        private async Task SaveReleasesWithArtistIds(DiscogsCollectionResponse collectionResponse)
        {
            try
            {
                var artistsTable = await _db.GetTable<Artist>();
                var artistsFromDb = await artistsTable.ToListAsync();
                foreach (var release in collectionResponse.releases)
                {
                    var artistIdForThisRelease = release.basic_information?.artists?.Select(x => x.id).FirstOrDefault();//only will save first artist for release, even though there may be many
                    var artistIdFromDb = artistsFromDb.Where(x => x.DiscogsArtistId == artistIdForThisRelease).Select(x => x.DiscogsArtistId).FirstOrDefault();

                    var releaseTable = await _db.GetTable<Release>();
                    var existingRelease = await releaseTable.Where(x => x.DiscogsReleaseId == release.id).FirstOrDefaultAsync();
                    if (existingRelease == null)
                    {
                        _ = await _db.InsertAsync(new Release
                        {
                            DiscogsArtistId = artistIdForThisRelease == artistIdFromDb ? artistIdForThisRelease : artistIdFromDb,
                            DiscogsReleaseId = release.id,//this id is the same as basicinformation.id
                            DiscogsMasterId = release.basic_information.master_id,
                            Genres = string.Join(",", release.basic_information.genres),//intending not to save list, but string.join - todo - task is made
                            Title = release.basic_information.title,
                            Year = release.basic_information.year,
                            DateAdded = release.date_added
                        });
                    }
                    else
                    {
                        _ = await _db.UpdateAsync(new Release
                        {
                            Id = existingRelease.Id,
                            DiscogsArtistId = artistIdForThisRelease == artistIdFromDb ? artistIdForThisRelease : artistIdFromDb,
                            DiscogsReleaseId = release.id,//this id is the same as basicinformation.id
                            DiscogsMasterId = release.basic_information.master_id,
                            Genres = string.Join(",", release.basic_information.genres),//intending not to save list, but string.join
                            Title = release.basic_information.title,
                            Year = release.basic_information.year,
                            DateAdded = release.date_added
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to SaveReleasesWithArtistIds:{ex.Message} ");
                throw;
            }
        }
        private async Task RemoveReleasesNoLongerInCollection(DiscogsCollectionResponse collectionResponse)
        {
            try
            {
                var releaseTable = await _db.GetTable<Release>();
                var existingReleases = await releaseTable.ToListAsync();
                // Get the DiscogsReleaseId values from the response
                var releasesInResponse = collectionResponse.releases.Select(r => r.id).ToList();

                // Identify releases in the database that are not in the response
                var releasesToRemove = existingReleases.Where(r => !releasesInResponse.Contains(r.DiscogsReleaseId)).ToList();

                // Remove the identified releases from the database
                foreach (var releaseToRemove in releasesToRemove)
                {
                    await _db.DeleteAsync(releaseToRemove);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at RemoveReleasesNoLongerInCollection:{ex.Message} ");
                throw;
            }
        }
        private async Task RemoveArtistsNoLongerInCollection(DiscogsCollectionResponse collectionResponse)
        {
            try
            {
                // Get the DiscogsArtistId values from the response
                var artistsInResponse = collectionResponse.releases
                    .SelectMany(r => r.basic_information.artists.Select(a => a.id))
                    .ToList();

                var artistsTable = await _db.GetTable<Artist>();
                // Identify artists in the database that are not in the response
                var artistsToRemove = await artistsTable.Where(a => !artistsInResponse.Contains(a.DiscogsArtistId)).ToListAsync();

                foreach (var artistToRemove in artistsToRemove)
                {
                    await _db.DeleteAsync(artistToRemove);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at RemoveReleasesNoLongerInCollection:{ex.Message} ");
                throw;
            }
        }
        #endregion
    }
}