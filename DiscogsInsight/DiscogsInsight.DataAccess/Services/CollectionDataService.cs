using DiscogsInsight.ApiIntegration.Contract;
using DiscogsInsight.ApiIntegration.Models.DiscogsResponseModels;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.Database.Contract;
using DiscogsInsight.Database.Entities;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.DataAccess.Services
{
    public class CollectionDataService : ICollectionDataService
    {
        private readonly ISQLiteAsyncConnection _db;
        private readonly IDiscogsApiService _discogsApiService;
        private readonly ILogger<CollectionDataService> _logger;

        public CollectionDataService(ISQLiteAsyncConnection db, IDiscogsApiService discogsApiService, ILogger<CollectionDataService> logger)
        {
            _db = db;
            _discogsApiService = discogsApiService;
            _logger = logger;
        }

        public async Task<List<DiscogsArtistIdAndName>> GetArtistsIdsAndNames()
        {
            var artistList = await _db.Table<Artist>().ToListAsync();

            return artistList.Select(x => 
            new DiscogsArtistIdAndName 
            {
                DiscogsArtistId = x.DiscogsArtistId,
                Name = x.Name 
            }).ToList();            
        }

        public async Task<List<DiscogsInsight.Database.Entities.Release>> GetReleases()
        {
            return await _db.Table<DiscogsInsight.Database.Entities.Release>().ToListAsync();
        }

        public async Task<bool> UpdateCollectionFromDiscogs()
        {
            try
            {
                var data = await _discogsApiService.GetCollectionFromDiscogsApi();
                if (data != null)
                {
                    return await SaveDiscogsCollectionResponse(data);
                }
                return false;
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
            await RemoveReleasesNoLongerInCollection(collectionResponse);
            await RemoveArtistsNoLongerInCollection(collectionResponse);
            //Todo: remove other database info when artist or release isnt in collection, could grow over time
            //musicbrainz images, track info, genre, etc
            return true;
        }

        private async Task SaveArtistsFromCollectionResponse(DiscogsCollectionResponse collectionResponse)
        {
            try
            {
                if (collectionResponse.releases != null)
                {
                    foreach (var release in collectionResponse.releases)
                    {
                        var artistsToSave = release.basic_information?.artists?.ToList();
                        foreach (var artist in artistsToSave)
                        {
                            var artistsTable = await _db.Table<DiscogsInsight.Database.Entities.Artist>().ToListAsync();
                            var existingArtist = artistsTable.Where(x => x.DiscogsArtistId == artist.id).FirstOrDefault();
                            if (existingArtist == null)
                            {
                                await _db.InsertAsync(new DiscogsInsight.Database.Entities.Artist
                                {
                                    DiscogsArtistId = artist.id,
                                    Name = artist.name,
                                });
                            }
                            else
                            {
                                await _db.UpdateAsync(new DiscogsInsight.Database.Entities.Artist
                                {
                                    Id = existingArtist.Id,
                                    DiscogsArtistId = artist.id,
                                    Name = artist.name
                                });
                            }
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
                var artistsTable = await _db.GetAllEntitiesAsListAsync<DiscogsInsight.Database.Entities.Artist>();
                if (collectionResponse.releases != null)
                {
                    foreach (var release in collectionResponse.releases)
                    {
                        var artistIdForThisRelease = release.basic_information?.artists?.Select(x => x.id).FirstOrDefault();//only will save first artist for release, even though there may be many
                        var artistIdFromDb = artistsTable.Where(x => x.DiscogsArtistId == artistIdForThisRelease).Select(x => x.DiscogsArtistId).FirstOrDefault();

                        var releaseTable = await _db.GetAllEntitiesAsListAsync<DiscogsInsight.Database.Entities.Release>();
                        var existingRelease = releaseTable.Where(x => x.DiscogsReleaseId == release.id).FirstOrDefault();
                        if (existingRelease == null)
                        {
                            _ = await _db.InsertAsync(new DiscogsInsight.Database.Entities.Release
                            {
                                DiscogsArtistId = artistIdForThisRelease == artistIdFromDb ? artistIdForThisRelease : artistIdFromDb,
                                DiscogsReleaseId = release.id,//this id is the same as basicinformation.id
                                DiscogsMasterId = release.basic_information.master_id,
                                Title = release.basic_information.title,
                                Year = release.basic_information.year,
                                DateAdded = release.date_added
                            });
                        }
                        //else //should not need to update every release? its just basic information at this point, and a collection update or first pull from api
                        //{
                        //here needs to not be instantiating a new release potentially
                        //    _ = await _db.UpdateAsync(new Release()
                        //    {
                        //        Id = existingRelease.Id,
                        //        DiscogsArtistId = artistIdForThisRelease == artistIdFromDb ? artistIdForThisRelease : artistIdFromDb,
                        //        DiscogsReleaseId = release.id,//this id is the same as basicinformation.id
                        //        DiscogsMasterId = release.basic_information.master_id,
                        //        Title = release.basic_information.title,
                        //        Year = release.basic_information.year,
                        //        DateAdded = release.date_added
                        //    });
                        //}

                        //save genres

                        //this is what it was doing before removing the service 
                        //var success = await _genresAndTagsDataService.SaveGenresFromDiscogsRelease(release, release.id, artistIdForThisRelease);


                        //public async Task<bool> SaveGenresFromDiscogsRelease(ResponseRelease responseRelease, int? discogsReleaseId, int? discogsArtistId)
                        //{
                        //    try
                        //    {
                        //        var discogsGenreTagsList = await _db.Table<DiscogsGenreTags>().ToListAsync();

                        //        if (responseRelease == null) return true;//dont think this is possible

                        //        var releaseGenresFromReleaseResponse = responseRelease.basic_information.genres;

                        //        if (releaseGenresFromReleaseResponse == null) return true;//dont want to error, there may just be no genres associated

                        //        var stylesNotInDatabaseAlready = releaseGenresFromReleaseResponse.Except(discogsGenreTagsList.Select(y => y.DiscogsTag)).ToList();

                        //        if (stylesNotInDatabaseAlready != null && stylesNotInDatabaseAlready.Count != 0)
                        //        {
                        //            foreach (var style in stylesNotInDatabaseAlready)
                        //            {
                        //                await _db.InsertAsync(new DiscogsGenreTags { DiscogsTag = style });
                        //            }

                        //            discogsGenreTagsList = await _db.Table<DiscogsGenreTags>().ToListAsync();
                        //        }

                        //        foreach (var style in releaseGenresFromReleaseResponse)
                        //        {
                        //            var genreTagId = discogsGenreTagsList.Where(x => x.DiscogsTag == style).Select(x => x.Id).FirstOrDefault();

                        //            await _db.InsertAsync(new DiscogsGenreTagToDiscogsRelease
                        //            {
                        //                DiscogsReleaseId = discogsReleaseId,
                        //                DiscogsArtistId = discogsArtistId,
                        //                DiscogsGenreTagId = genreTagId
                        //            });
                        //        }

                        //        return true;
                        //    }
                        //    catch (Exception)
                        //    {
                        //        throw;
                        //    }


                        //}
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
                var releaseTable = await _db.GetAllEntitiesAsListAsync<DiscogsInsight.Database.Entities.Release>();
                // Get the DiscogsReleaseId values from the response
                var releasesInResponse = collectionResponse.releases.Select(r => r.id).ToList();

                // Identify releases in the database that are not in the response
                var releasesToRemove = releaseTable.Where(r => !releasesInResponse.Contains(r.DiscogsReleaseId)).ToList();

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

                var artistsTable = await _db.GetAllEntitiesAsListAsync<DiscogsInsight.Database.Entities.Artist>();
                // Identify artists in the database that are not in the response
                var artistsToRemove = artistsTable.Where(a => !artistsInResponse.Contains(a.DiscogsArtistId)).ToList();

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

        public async Task<bool> CheckCollectionIsSeededOrSeed()
        {
            var oneRecordQuery = @$"SELECT DiscogsReleaseId FROM Release LIMIT 1;";
            var oneRecord = await _db.QueryAsync<DiscogsReleaseIdClass>(oneRecordQuery);

            if (oneRecord == null || (oneRecord != null && oneRecord.Select(x => x.DiscogsReleaseId).FirstOrDefault() == 0)) 
            {
                //SEEEEEEEEEED
                var data = await _discogsApiService.GetCollectionFromDiscogsApi();
                if (data != null)
                {
                    await SaveDiscogsCollectionResponse(data);
                }
            }
            return true;
        }
    }
}