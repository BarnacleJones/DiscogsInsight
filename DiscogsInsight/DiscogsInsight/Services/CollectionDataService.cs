using DiscogsInsight.DataModels;
using DiscogsInsight.ResponseModels;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.Services
{
    public class CollectionDataService
    {
        private readonly DiscogsInsightDb _db;
        private readonly ILogger<CollectionDataService> _logger;
        public CollectionDataService(DiscogsInsightDb db, ILogger<CollectionDataService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<T>> GetAllEntitiesAsync<T>() where T : IDatabaseEntity, new()
        {
            var entities = await _db.GetTable<T>();
            return await entities.ToListAsync();
        }

        public async Task PurgeEntireDb()
        {
            await _db.Purge();
        }

        #region Artist Response Methods
        public async Task<bool> SaveDiscogsArtistResponse(DiscogsArtistResponse releaseResponse)
        {
            try
            {
                var artistsTable = await _db.GetTable<Artist>();
                var existingArtist = artistsTable.Where(x => x.DiscogsArtistId == releaseResponse.id).FirstOrDefaultAsync().Result;
                if (existingArtist == null)
                {
                    //dont want to store the artist if not in db already
                    throw new Exception($"Unhandled exception: Artist {releaseResponse.id} not in database not able to store info.");
                }
                else
                {
                    existingArtist.Profile = releaseResponse.profile;
                    await _db.UpdateAsync(existingArtist);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
                throw new Exception($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
            }
            return true;
        }

        #endregion 

        #region Release Response Methods
        public async Task<bool> SaveDiscogsReleaseResponse(DiscogsReleaseResponse releaseResponse)
        {
            //save tracklists, make tracklist entity
            try
            {
                var releaseTable = await _db.GetTable<Release>();
                var existingRelease = releaseTable.Where(x => x.DiscogsReleaseId == releaseResponse.id).FirstOrDefaultAsync().Result;
                var tracksTable = await _db.GetTable<Track>();
                var existingTracks = tracksTable.Where(x => x.DiscogsReleaseId == releaseResponse.id).ToListAsync().Result;
                if (existingRelease == null)
                    //at this stage, dont want to store the release info if not in db already
                    throw new Exception($"Unhandled exception: Release {releaseResponse.id} not in database not able to store info.");

                existingRelease.ReleaseCountry = releaseResponse.country;
                await _db.UpdateAsync(existingRelease);

                if (!existingTracks.Any())
                {
                    foreach (var track in releaseResponse.tracklist)
                    {
                        await _db.SaveItemAsync<Track>(new Track
                        {
                            DiscogsArtistId = existingRelease.DiscogsArtistId,
                            DiscogsMasterId = existingRelease.DiscogsMasterId,
                            DiscogsReleaseId = releaseResponse.id,
                            Duration = track.duration,
                            Title = track.title,
                            Position = track.position
                        });
                    }
                }     
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
                throw new Exception($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
            }
            return true;
        }

        #endregion

        #region Collection Response Methods
        public async Task<bool> SaveDiscogsCollectionResponse(DiscogsCollectionResponse collectionResponse)
        {            
            await SaveArtistsFromCollectionResponse(collectionResponse);
            await SaveReleasesWithArtistIds(collectionResponse);
            await RemoveReleasesNoLongerInCollection(collectionResponse);
            await RemoveArtistsNoLongerInCollection(collectionResponse);
            return true;
        }
        private async Task RemoveReleasesNoLongerInCollection(DiscogsCollectionResponse collectionResponse)
        {
            try
            {
                var releaseTable = await _db.GetTable<Release>();
                var existingReleases = releaseTable.ToListAsync().Result;

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
                throw new Exception($"Exception at RemoveReleasesNoLongerInCollection:{ex.Message} ");
            }
        }

        private async Task RemoveArtistsNoLongerInCollection(DiscogsCollectionResponse collectionResponse)
        {
            try
            {
                var artistsTable = await _db.GetTable<Artist>();
                var existingArtists = artistsTable.ToListAsync().Result;

                // Get the DiscogsArtistId values from the response
                var artistsInResponse = collectionResponse.releases
                    .SelectMany(r => r.basic_information.artists.Select(a => a.id))
                    .ToList();

                // Identify artists in the database that are not in the response
                var artistsToRemove = existingArtists.Where(a => !artistsInResponse.Contains(a.DiscogsArtistId)).ToList();

                // Remove the identified artists from the database
                foreach (var artistToRemove in artistsToRemove)
                {
                    await _db.DeleteAsync(artistToRemove);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at RemoveReleasesNoLongerInCollection:{ex.Message} ");
                throw new Exception($"Exception at RemoveArtistsNoLongerInCollection:{ex.Message} ");
            }
        }
        private async Task SaveReleasesWithArtistIds(DiscogsCollectionResponse collectionResponse)
        {
            try
            {
                var artistsTable = await _db.GetTable<Artist>();
                var artistsFromDb = artistsTable.ToListAsync().Result;
                foreach (var release in collectionResponse.releases)
                {
                    var artistIdForThisRelease = release.basic_information.artists.Select(x => x.id).FirstOrDefault();//only will save first artist for release, even though there may be many
                    var artistIdFromDb = artistsFromDb.Where(x => x.DiscogsArtistId == artistIdForThisRelease).Select(x => x.DiscogsArtistId).FirstOrDefault();

                    var releaseTable = await _db.GetTable<Release>();
                    var existingRelease = releaseTable.Where(x => x.DiscogsReleaseId == release.id).FirstOrDefaultAsync();
                    if (await existingRelease == null)
                    {

                        await _db.InsertAsync(new Release
                        {
                            DiscogsArtistId = (artistIdForThisRelease == artistIdFromDb) ? artistIdForThisRelease : null,
                            DiscogsReleaseId = release.id,//this id is the same as basicinformation.id
                            DiscogsMasterId = release.basic_information.master_id,
                            Genres = string.Join(",", release.basic_information.genres),//intending not to save list, but string.join
                            Title = release.basic_information.title,
                            Year = release.basic_information.year,
                            DateAdded = release.date_added
                        });

                    }
                    else
                    {
                        await _db.UpdateAsync(new Release
                        {
                            Id = existingRelease.Id,
                            DiscogsArtistId = (artistIdForThisRelease == artistIdFromDb) ? artistIdForThisRelease : null,
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
                _logger.LogError($"Exception at SaveReleasesWithArtistIds:{ex.Message} ");
                throw new Exception($"Exception at SaveReleasesWithArtistIds:{ex.Message} ");
            }
        }

        private async Task SaveArtistsFromCollectionResponse(DiscogsCollectionResponse collectionResponse)
        {
            try
            {
                foreach (var release in collectionResponse.releases)
                {
                    var artistsToSave = release.basic_information.artists.ToList();
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
                throw new Exception($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
            }
        }


        #endregion
    }
}
