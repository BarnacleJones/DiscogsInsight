using DiscogsInsight.ApiIntegration.Contract;
using DiscogsInsight.ApiIntegration.Models.MusicBrainzResponseModels;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.Database.Contract;
using DiscogsInsight.Database.Entities;
using Microsoft.Extensions.Logging;
using Artist = DiscogsInsight.Database.Entities.Artist;

namespace DiscogsInsight.DataAccess.Services
{
    public class ArtistDataService : IArtistDataService
    {
        private readonly IDiscogsInsightDb _db;
        private readonly IDiscogsApiService _discogsApiService;
        private readonly IMusicBrainzApiService _musicBrainzApiService;
        private readonly ILogger<ArtistDataService> _logger;
        private readonly ITagsDataService _tagsDataService;

        public ArtistDataService(IDiscogsInsightDb db, 
            IDiscogsApiService discogsApiService, 
            ILogger<ArtistDataService> logger, 
            IMusicBrainzApiService musicBrainzApiService,
            ITagsDataService tagsDataService)
        {
            _db = db;
            _discogsApiService = discogsApiService;
            _logger = logger;
            _musicBrainzApiService = musicBrainzApiService;
            _tagsDataService = tagsDataService;
        }

        private async Task<Artist?> GetArtistByDiscogsIdFromDb(int discogsArtistId)
        {
            var artists = await _db.GetAllEntitiesAsListAsync<Artist>();
            return artists.FirstOrDefault(x => x.DiscogsArtistId == discogsArtistId);
        }

        public async Task<List<Artist>?> GetAllArtistsFromDatabase()
        {
            var artists = await _db.GetAllEntitiesAsListAsync<Artist>();
            return artists.ToList();
        }

        public async Task<Artist?> GetArtistByDiscogsId(int? discogsArtistId, bool fetchAndSaveApiData = true)
        {
            if (discogsArtistId == null) { return new Artist { Name = "No Artist Id Supplied" }; }
            
            var existingArtist = await GetArtistByDiscogsIdFromDb(discogsArtistId.Value);

            if (existingArtist == null)
            {
                //dont want to store the artist if not in db already, it hasnt been in the main discogs collection call
                //i dont know how would one get here...
                throw new Exception($"Unhandled exception: Artist {discogsArtistId} not in db. It might be a 'various artist' issue, or refresh your database");
            }
            if (existingArtist.Name?.ToLower() == "various")
            {
                //dont fetch api data - 'Various' 404s for discogs, and causes bad data with MusicBrainz. 
                return existingArtist;
            }
            if (fetchAndSaveApiData && !existingArtist.HasAllApiData)
            {
                if (string.IsNullOrWhiteSpace(existingArtist.Profile))
                {
                    var discogsResult = await _discogsApiService.GetArtistFromDiscogs(discogsArtistId.Value);
                    //Add additional properties wanted from Artist Discogs call here...profile is really the only useful one here
                    existingArtist.Profile = discogsResult.profile;
                }
                if (existingArtist.MusicBrainzArtistId == null && existingArtist.Name != null)
                {
                    var musicBrainzResult = await _musicBrainzApiService.GetInitialArtistFromMusicBrainzApi(existingArtist.Name);
                    var success = await SetMusicBrainzArtistDataForSavingAndSaveTagsFromArtistResponse(musicBrainzResult, existingArtist);
                }

                existingArtist.HasAllApiData = true;
                await _db.UpdateAsync(existingArtist);
            }

            return existingArtist;
        }

        #region Private MusicBrainz Artist Functions


        private async Task<bool> SetMusicBrainzArtistDataForSavingAndSaveTagsFromArtistResponse(MusicBrainzInitialArtist artistResponse, Artist existingArtist)
        {               
            //**this makes an assumption that can cause bad data**
            //Artists in the response is a list, there are similar named artists in the list
            //It looks like the first in the list is closest match
            //Ability to correct this data is on the release card

            var musicBrainsArtistId = artistResponse.Artists.Select(x => x.Id).FirstOrDefault();

            existingArtist.MusicBrainzArtistId = musicBrainsArtistId;

            var artistArea = artistResponse.Artists
                                                    .Where(x => x.Id == musicBrainsArtistId)
                                                    .Select(x => x.Area)
                                                    .FirstOrDefault();
            var artistBeginArea = artistResponse.Artists
                                                    .Where(x => x.Id == musicBrainsArtistId)
                                                    .Select(x => x.BeginArea)
                                                    .FirstOrDefault();
                    
            if (artistBeginArea != null)
            {
                var isACity = artistBeginArea.Type == "City";
                var isACountry = artistBeginArea.Type == "Country";
                if (isACity)
                {
                    existingArtist.City = artistBeginArea.Name;
                }
                if (isACountry)
                {
                    existingArtist.Country = artistBeginArea.Name;
                }
            }
                    
            if (artistArea != null)
            {
                var isACity = artistArea.Type == "City";
                var isACountry = artistArea.Type == "Country";
                if (isACountry)
                {
                    existingArtist.Country = artistArea.Name;
                }
                if (isACity)
                {
                    existingArtist.City = artistArea.Name;
                }
            }

            existingArtist.StartYear = artistResponse.Artists
                                                    .Where(x => x.Id == musicBrainsArtistId)
                                                    .Select(x => x.LifeSpan?.Begin)
                                                    .FirstOrDefault();

            existingArtist.EndYear = artistResponse.Artists
                                                    .Where(x => x.Id == musicBrainsArtistId)
                                                    .Select(x => x.LifeSpan?.End)
                                                    .FirstOrDefault();

                   

            //save tags - yes it is saving to joining table too before saving the actual artist table information, but its saving writes to the db....
            var success = await _tagsDataService.SaveTagsByMusicBrainzArtistId(artistResponse, musicBrainsArtistId ?? "");

            return true;  
        }

        public async Task<List<MusicBrainzArtistRelease>> GetArtistsReleasesByMusicBrainzArtistId(string musicBrainzArtistId)
        {
            try
            {
                var allReleasesTable = await _db.GetAllEntitiesAsListAsync<MusicBrainzArtistToMusicBrainzRelease>();

                var allReleasesByArtist = allReleasesTable.Where(x => x.MusicBrainzArtistId == musicBrainzArtistId).ToList();

                return allReleasesByArtist.Select(x => new MusicBrainzArtistRelease
                {
                    MusicBrainzReleaseName = x.MusicBrainzReleaseName ??  " " ,
                    ReleaseYear = x.ReleaseYear ?? " ",
                    Status = x.Status ?? " ",
                }).ToList();

            }
            catch (Exception)
            {
                throw;
            }
        }


        #endregion

        #region Artist Data Correction
        public async Task<List<PossibleArtistsFromMusicBrainzApi>> GetPossibleArtistsForDataCorrectionFromDiscogsReleaseId(int? discogsReleaseId)
        {
            //get artist name from release id
            var allReleases = await _db.GetAllEntitiesAsListAsync<Database.Entities.Release>();
            var thisRelease = allReleases.Where(x => x.DiscogsReleaseId == discogsReleaseId).FirstOrDefault();//there could be more than one
            

            var allArtists = await _db.GetAllEntitiesAsListAsync<Database.Entities.Artist>();
            var recordForThisArtist = allArtists.Where(x => x.DiscogsArtistId == thisRelease.DiscogsArtistId).FirstOrDefault();//could be more than one
            
            
            var musicBrainzApiCall = await _musicBrainzApiService.GetInitialArtistFromMusicBrainzApi(recordForThisArtist.Name);
            
            if (musicBrainzApiCall != null && musicBrainzApiCall.Artists != null)
            {
                var potentialArtists = musicBrainzApiCall.Artists.Select(x => new PossibleArtistsFromMusicBrainzApi
                {
                    ArtistName = x.Name ?? "No Name",
                    CorrectArtistMusicBrainzId = x.Id ?? "",
                    Country = x.Country ?? "Unknown",
                    Disambiguation = x.Disambiguation ?? "Unknown",
                }).ToList();
                return potentialArtists;
            }

            return [];
        }
        public async Task<bool> DeleteExistingArtistDataAndUpdateToChosenMusicBrainzArtistFromMusicBrainzId(int? discogsReleaseId, string newAritstMusicBrainzId)
        {
            await DeleteAllBadArtistAndReleaseData(discogsReleaseId, newAritstMusicBrainzId);

            var releaseTableList = await _db.GetAllEntitiesAsListAsync<Database.Entities.Release>();
            var discogsArtistFromReleaseId = releaseTableList.Where(x => x.DiscogsReleaseId == discogsReleaseId).FirstOrDefault().DiscogsArtistId;
          
            if (newAritstMusicBrainzId == null)
            {
                return false;//this shouldnt happen?
            }

            var artistCallResponse = await _musicBrainzApiService.GetArtistFromMusicBrainzApiUsingArtistId(newAritstMusicBrainzId);

            await UpdateArtistTableWithCorrectedData(newAritstMusicBrainzId, artistCallResponse);

            await UpdateMusicBrainzTablesWithCorrectedData(newAritstMusicBrainzId, discogsArtistFromReleaseId, artistCallResponse);
            
            return true;
        }

        private async Task UpdateMusicBrainzTablesWithCorrectedData(string newAritstMusicBrainzId, int? discogsArtistFromReleaseId, MusicBrainzArtist artistCallResponse)
        {
            var releasesByArtist = artistCallResponse.Releases?.ToList();
            var releaseGroupsByArtist = artistCallResponse.ReleaseGroups?.ToList();
            var existingJoins = await _db.GetAllEntitiesAsListAsync<MusicBrainzArtistToMusicBrainzRelease>();
            var existingReleasesForThisArtistList = existingJoins.Where(x => x.MusicBrainzArtistId == newAritstMusicBrainzId).Select(x => x.MusicBrainzReleaseId).ToList();

            if (releasesByArtist != null && releasesByArtist.Count != 0)
            {
                foreach (var artistsRelease in releasesByArtist)
                {
                    if (existingReleasesForThisArtistList.Contains(artistsRelease.Id))
                    {
                        continue;//already exists
                    }
                    var artistIdToReleaseId = new MusicBrainzArtistToMusicBrainzRelease
                    {
                        MusicBrainzArtistId = newAritstMusicBrainzId,
                        DiscogsArtistId = discogsArtistFromReleaseId ?? 0,
                        MusicBrainzReleaseId = artistsRelease.Id,
                        MusicBrainzReleaseName = artistsRelease.Title,
                        ReleaseYear = artistsRelease.Date,
                        Status = artistsRelease.Status,
                        IsAReleaseGroupGroupId = false //different cover art endpoint for release group id vs release id
                    };
                    await _db.InsertAsync(artistIdToReleaseId);
                }

            }
            if (releaseGroupsByArtist != null && releaseGroupsByArtist.Count != 0)
            {
                foreach (var artistsReleaseGroup in releaseGroupsByArtist)
                {
                    if (existingReleasesForThisArtistList.Contains(artistsReleaseGroup.Id))
                    {
                        continue;//already exists
                    }
                    var artistIdToReleaseId = new MusicBrainzArtistToMusicBrainzRelease
                    {
                        MusicBrainzArtistId = newAritstMusicBrainzId,
                        DiscogsArtistId = discogsArtistFromReleaseId ?? 0,
                        MusicBrainzReleaseId = artistsReleaseGroup.Id,
                        MusicBrainzReleaseName = artistsReleaseGroup.Title,
                        ReleaseYear = artistsReleaseGroup.FirstReleaseDate,
                        Status = artistsReleaseGroup.PrimaryType,
                        IsAReleaseGroupGroupId = true //different cover art endpoint for release group id vs release id
                    };
                    await _db.InsertAsync(artistIdToReleaseId);
                }
            }
        }

        private async Task UpdateArtistTableWithCorrectedData(string newAritstMusicBrainzId, MusicBrainzArtist artistCallResponse)
        {
            var allArtists = await _db.GetAllEntitiesAsListAsync<Database.Entities.Artist>();
            var recordForThisArtist = allArtists.Where(x => x.MusicBrainzArtistId == newAritstMusicBrainzId).ToList();//could be more than one

            var artistBeginArea = artistCallResponse.BeginArea;
            var artistArea = artistCallResponse.Area;
            foreach (var artist in recordForThisArtist)
            {

                if (artistBeginArea != null)
                {
                    var isACity = artistBeginArea.Type == "City";
                    var isACountry = artistBeginArea.Type == "Country";
                    if (isACity)
                    {
                        artist.City = artistBeginArea.Name;
                    }
                    if (isACountry)
                    {
                        artist.Country = artistBeginArea.Name;
                    }
                    else
                    {
                        artist.City = artistBeginArea.Name;
                    }
                }

                if (artistArea != null)
                {
                    var isACity = artistArea.Type == "City";
                    var isACountry = artistArea.Type == "Country";
                    if (isACountry)
                    {
                        artist.Country = artistArea.Name;
                    }
                    if (isACity)
                    {
                        artist.City = artistArea.Name;
                    }
                    else
                    {
                        artist.Country = artistArea.Name;
                    }

                }

                artist.StartYear = artistCallResponse.LifeSpan.Begin;
                artist.EndYear = artistCallResponse.LifeSpan.End;

                await _db.UpdateAsync(artist);
            }
        }

        private async Task DeleteAllBadArtistAndReleaseData(int? discogsReleaseId, string newAritstMusicBrainzId)
        {
            var allReleases = await _db.GetAllEntitiesAsListAsync<Database.Entities.Release>();
            var thisReleaseList = allReleases.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();//there could be more than one

            foreach (var release in thisReleaseList)
            {
                release.MusicBrainzReleaseId = null;
                release.HasAllApiData = false;
                release.ArtistHasBeenManuallyCorrected = true;
                await _db.UpdateAsync(release);
            }

            var allArtists = await _db.GetAllEntitiesAsListAsync<Database.Entities.Artist>();
            var artistId = thisReleaseList.FirstOrDefault().DiscogsArtistId;
            var recordsForThisArtist = allArtists.Where(x => x.DiscogsArtistId == artistId).ToList();//could be more than one

            var badMusicBrainzArtistId = recordsForThisArtist.FirstOrDefault().MusicBrainzArtistId;

            foreach (var artist in recordsForThisArtist)
            {
                artist.MusicBrainzArtistId = newAritstMusicBrainzId;
                artist.City = null;
                artist.Country = null;
                artist.StartYear = null;
                artist.EndYear = null;
                await _db.UpdateAsync(artist);
            }

            var artistToTagsTable = await _db.GetAllEntitiesAsListAsync<MusicBrainzArtistToMusicBrainzTags>();
            var artistsToTagsToRemove = artistToTagsTable.Where(x => x.MusicBrainzArtistId == badMusicBrainzArtistId).ToList();
            foreach (var artistToTag in artistsToTagsToRemove)
            {
                await _db.DeleteAsync(artistToTag);
            }

            var artistToReleaseTable = await _db.GetAllEntitiesAsListAsync<MusicBrainzArtistToMusicBrainzRelease>();
            var artistsToReleasesToRemove = artistToReleaseTable.Where(x => x.MusicBrainzArtistId == badMusicBrainzArtistId).ToList();
            var listOfReleaseIdsTiedToThisArtist = new List<string>();
            foreach (var artistRecord in artistsToReleasesToRemove)
            {
                listOfReleaseIdsTiedToThisArtist.Add(artistRecord.MusicBrainzReleaseId);
                await _db.DeleteAsync(artistRecord);
            }


            var coverImageTable = await _db.GetAllEntitiesAsListAsync<MusicBrainzReleaseToCoverImage>();
            var coverImagesForThisRelease = thisReleaseList.FirstOrDefault().MusicBrainzReleaseId;
            var coverImagesStoredForBadReleases = coverImageTable.Where(x => listOfReleaseIdsTiedToThisArtist.Contains(x.MusicBrainzReleaseId)).ToList();
            foreach (var coverImageRecord in coverImagesStoredForBadReleases)
            {
                await _db.DeleteAsync(coverImageRecord);
            }
        }

        #endregion
    }

}
