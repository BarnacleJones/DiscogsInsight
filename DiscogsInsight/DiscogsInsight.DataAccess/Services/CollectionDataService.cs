using DiscogsInsight.ApiIntegration.Contract;
using DiscogsInsight.ApiIntegration.Models.DiscogsResponseModels;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.Database.Contract;
using DiscogsInsight.Database.Entities;
using Microsoft.Extensions.Logging;
using System.Linq;

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
            return await MakeCollectionApiCallAndSaveDiscogsCollectionResponse();
        }

        public async Task<bool> CheckCollectionIsSeededOrSeed()
        {
            var oneRecordQuery = @$"SELECT DiscogsReleaseId FROM Release LIMIT 1;";
            var oneRecord = await _db.QueryAsync<DiscogsReleaseIdClass>(oneRecordQuery);

            if (oneRecord == null || (oneRecord != null && oneRecord.Select(x => x.DiscogsReleaseId).FirstOrDefault() == 0))
            {
                return await MakeCollectionApiCallAndSaveDiscogsCollectionResponse();
            }
            return true;
        }
        private async Task<bool> MakeCollectionApiCallAndSaveDiscogsCollectionResponse()
        {
            var data = await _discogsApiService.GetCollectionFromDiscogsApi();
            if (data != null)
            {
                return await SaveDiscogsCollectionResponse(data);
            }
            return false;
        }


        private async Task<bool> SaveDiscogsCollectionResponse(DiscogsCollectionResponse collectionResponse)
        {
            await SaveCollectionDataFromDiscogsCollectionResponse(collectionResponse);

            await RemoveReleasesNoLongerInCollection(collectionResponse);
            await RemoveArtistsNoLongerInCollection(collectionResponse);

            return true;
        }


        //single function that loops through the releases, makes an entry of artist (just id and name) and same for release doing SaveReleasesWithArtistIds but simpler
        private async Task SaveCollectionDataFromDiscogsCollectionResponse(DiscogsCollectionResponse collectionResponse)
        {
            if (collectionResponse.releases != null && collectionResponse.releases.Count > 0)
            {          
                var discogsArtistIdToArtistToInsert = new Dictionary<int, Artist>();
                var discogsArtistIdToReleaseToInsert = new Dictionary<int, Release>();

                var existingArtistIds = await _db.AllDiscogsArtistIdsInDb();
                var existingReleaseIds = await _db.AllDiscogsReleaseIdsInDb();

                var apiReleases = collectionResponse.releases.Where(x => x.id.HasValue).ToList();

                foreach (var release in apiReleases)
                {
                    if (!existingReleaseIds.Contains(release.id.Value))
                    {
                        var artistDataForThisRelease = release.basic_information?.artists?.FirstOrDefault();//only will save first artist for release, even though there may be many

                        //Save to Release
                        discogsArtistIdToReleaseToInsert[release.id.Value] = new DiscogsInsight.Database.Entities.Release
                        {
                            DiscogsArtistId = artistDataForThisRelease?.id ?? 0,
                            DiscogsReleaseId = release.id,//this id is the same as basicinformation.id
                            DiscogsMasterId = release.basic_information.master_id,
                            Title = release.basic_information.title,
                            Year = release.basic_information.year,
                            DateAdded = release.date_added
                        };

                        //Save to Artist
                        if (artistDataForThisRelease.id.HasValue && !existingArtistIds.Contains(artistDataForThisRelease.id.Value))
                        {                            
                            discogsArtistIdToArtistToInsert[artistDataForThisRelease.id.Value] = new Artist 
                            {
                                DiscogsArtistId = artistDataForThisRelease.id.Value,
                                Name = artistDataForThisRelease.name,
                            };
                        }

                        //Save to DiscogsGenres

                        //Save to DiscogsGenresToDiscogsRelease

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



                    }
                    else
                    {
                        //do some logic to
                        //add to list to remove it from the collection
                    }
                  
                }

                await _db.InsertAllAsync<Artist>(discogsArtistIdToArtistToInsert.Values);
                await _db.InsertAllAsync<Release>(discogsArtistIdToReleaseToInsert.Values);
            }
        
        }
        private async Task RemoveReleasesNoLongerInCollection(DiscogsCollectionResponse collectionResponse)
        {
            try
            {
                var releaseTable = await _db.Table<DiscogsInsight.Database.Entities.Release>().ToListAsync();
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
    }
}