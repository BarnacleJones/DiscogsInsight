using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using DiscogsInsight.ApiIntegration.MusicBrainzResponseModels;
using DiscogsInsight.ApiIntegration.Services;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Entities;
using DiscogsInsight.DataAccess.Models;
using Microsoft.Extensions.Logging;
using Artist = DiscogsInsight.DataAccess.Entities.Artist;

namespace DiscogsInsight.DataAccess.Services
{
    public class ArtistDataService : IArtistDataService
    {
        private readonly IDiscogsInsightDb _db;
        private readonly DiscogsApiService _discogsApiService;
        private readonly MusicBrainzApiService _musicBrainzApiService;
        private readonly ILogger<ArtistDataService> _logger;
        private readonly ITagsDataService _tagsDataService;

        public ArtistDataService(IDiscogsInsightDb db, 
            DiscogsApiService discogsApiService, 
            ILogger<ArtistDataService> logger, 
            MusicBrainzApiService musicBrainzApiService,
            ITagsDataService tagsDataService)
        {
            _db = db;
            _discogsApiService = discogsApiService;
            _logger = logger;
            _musicBrainzApiService = musicBrainzApiService;
            _tagsDataService = tagsDataService;
        }

        public async Task<Artist?> GetArtistByDiscogsId(int discogsArtistId)
        {
            var artists = await _db.GetAllEntitiesAsListAsync<Artist>();
            return artists.FirstOrDefault(x => x.DiscogsArtistId == discogsArtistId);
        }

        public async Task<List<Artist>?> GetArtists()
        {
            var artists = await _db.GetAllEntitiesAsListAsync<Artist>();
            return artists.ToList();
        }

        public async Task<Artist?> GetArtist(int? discogsArtistId, bool fetchAndSaveApiData = true)
        {
            if (discogsArtistId == null) { return new Artist { Name = "No Artist Id Supplied" }; }
            bool saved = true;

            if (fetchAndSaveApiData && discogsArtistId.HasValue)
            {
                saved = await GetAndSaveDiscogsArtistData((int)discogsArtistId);
                var musicBrainzId = await GetAndSaveInitialMusicBrainzArtistData((int)discogsArtistId);
                saved = !string.IsNullOrWhiteSpace(musicBrainzId);
            }

            return saved 
                ? await GetArtistByDiscogsId((int)discogsArtistId)
                : new Artist() { Name="Error saving artist"};
        }

        #region Private Discogs Artist Functions

        private async Task<bool> GetAndSaveDiscogsArtistData(int discogsArtistId)
        {
            var artistsTable = await _db.GetAllEntitiesAsListAsync<Artist>();
            var existingArtist = artistsTable.Where(x => x.DiscogsArtistId == discogsArtistId).FirstOrDefault();
            if (existingArtist == null)
            {
                //dont want to store the artist if not in db already
                //it hasnt been in the main discogs collection call
                //i dont know how would one get here...
                throw new Exception($"Unhandled exception: Artist {discogsArtistId} not in db. It might be a various artist issue, or refresh your database");
            }
            var existingArtistNameIsVarious = existingArtist.Name?.ToLower() == "various";
            
            if (existingArtistNameIsVarious) { return true; } //discogs id of various artists doesnt go anywhere - causes 404s

            if (string.IsNullOrWhiteSpace(existingArtist.Profile) && !existingArtistNameIsVarious)
            {
                var result = await _discogsApiService.GetArtistFromDiscogs(discogsArtistId);
                var saved = await SaveDiscogsArtistResponse(existingArtist, result);
                return saved;
            }
            return true;
        }

        private async Task<bool> SaveDiscogsArtistResponse(Artist existingArtist, DiscogsArtistResponse artistResponse)
        {
            try
            {               
                //additional properties from discogs artist call that arent on the main collection call
                existingArtist.Profile = artistResponse.profile;
                await _db.UpdateAsync(existingArtist);                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at SaveDiscogsArtistResponse:{ex.Message} ");
                throw;
            }
        }
        #endregion

        #region Private MusicBrainz Artist Functions

        private async Task<string> GetAndSaveInitialMusicBrainzArtistData(int discogsArtistId)
        {
            try
            {
                var existingArtist = await GetArtistByDiscogsId(discogsArtistId);
                if (existingArtist == null)
                {
                    //dont want to store the artist if not in db already
                    //it hasnt been in the main discogs collection call
                    //i dont know how would one get here...
                    throw new Exception($"Unhandled exception: Artist {discogsArtistId} not in db. It might be a various artist issue, or refresh your database");
                }

                var existingArtistNameIsVarious = existingArtist.Name?.ToLower() == "various";

                if (existingArtistNameIsVarious) { return "Various"; } //trying to extrapolate the correct various artist is going to be very difficult

                if (existingArtist.MusicBrainzArtistId == null && existingArtist.Name != null)
                {
                    var result = await _musicBrainzApiService.GetInitialArtistFromMusicBrainzApi(existingArtist.Name);
                    return await SaveMusicBrainzInitialArtistResponseAndReturnMusicBrainzArtistId(result, discogsArtistId);
                }

                return existingArtist.MusicBrainzArtistId ?? "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<string> SaveMusicBrainzInitialArtistResponseAndReturnMusicBrainzArtistId(MusicBrainzInitialArtist artistResponse, int discogsArtistId)
        {
            try
            {
                var existingArtist = await GetArtistByDiscogsId(discogsArtistId);
                if (existingArtist == null)
                {
                    //How would this happen...
                    throw new Exception($"Discogs Artist Id {discogsArtistId} does not return a artist from the database. In function SaveMusicBrainzInitialArtistResponseAndReturnMusicBrainzArtistId");
                }
                else
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

                    await _db.UpdateAsync(existingArtist);

                    //save tags
                    var success = await _tagsDataService.SaveTagsByMusicBrainzArtistId(artistResponse, musicBrainsArtistId ?? "");

                    return musicBrainsArtistId ?? "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
                throw;
            }
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
            var allReleases = await _db.GetAllEntitiesAsListAsync<Entities.Release>();
            var thisRelease = allReleases.Where(x => x.DiscogsReleaseId == discogsReleaseId).FirstOrDefault();//there could be more than one
            

            var allArtists = await _db.GetAllEntitiesAsListAsync<Entities.Artist>();
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

            var releaseTableList = await _db.GetAllEntitiesAsListAsync<Entities.Release>();
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

            if (releasesByArtist != null && releasesByArtist.Any())
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
            if (releaseGroupsByArtist != null && releaseGroupsByArtist.Any())
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
            var allArtists = await _db.GetAllEntitiesAsListAsync<Entities.Artist>();
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
            var allReleases = await _db.GetAllEntitiesAsListAsync<Entities.Release>();
            var thisReleaseList = allReleases.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();//there could be more than one

            foreach (var release in thisReleaseList)
            {
                release.MusicBrainzReleaseId = null;
                release.HasAllApiData = false;
                release.ArtistHasBeenManuallyCorrected = true;
                await _db.UpdateAsync(release);
            }

            var allArtists = await _db.GetAllEntitiesAsListAsync<Entities.Artist>();
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
