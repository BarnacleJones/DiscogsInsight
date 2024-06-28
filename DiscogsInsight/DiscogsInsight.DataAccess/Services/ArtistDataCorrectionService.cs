using DiscogsInsight.ApiIntegration.Contract;
using DiscogsInsight.ApiIntegration.Models.MusicBrainzResponseModels;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.Database.Contract;
using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.DataAccess.Services
{
    public class ArtistDataCorrectionService : IArtistDataCorrectionService
    {
        private readonly ISQLiteAsyncConnection _db;
        private readonly IMusicBrainzApiService _musicBrainzApiService;

        public ArtistDataCorrectionService(ISQLiteAsyncConnection db,
            IMusicBrainzApiService musicBrainzApiService)
        {
            _db = db;
            _musicBrainzApiService = musicBrainzApiService;
        }

        public async Task<List<PossibleArtistsFromMusicBrainzApi>> GetPossibleArtistsForDataCorrectionFromDiscogsReleaseId(int? discogsReleaseId)
        {
            //get artist name from release id
            var allReleases = await _db.Table<Database.Entities.Release>().ToListAsync();
            var thisRelease = allReleases.Where(x => x.DiscogsReleaseId == discogsReleaseId).FirstOrDefault();//there could be more than one


            var allArtists = await _db.Table<Database.Entities.Artist>().ToListAsync();
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

            var releaseTableList = await _db.Table<Database.Entities.Release>().ToListAsync();
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
            var existingJoins = await _db.Table<MusicBrainzArtistToMusicBrainzRelease>().ToListAsync();
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
            var allArtists = await _db.Table<Database.Entities.Artist>().ToListAsync();
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
            var allReleases = await _db.Table<Database.Entities.Release>().ToListAsync();
            var thisReleaseList = allReleases.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();//there could be more than one

            foreach (var release in thisReleaseList)
            {
                release.MusicBrainzReleaseId = null;
                release.HasAllApiData = false;
                release.ArtistHasBeenManuallyCorrected = true;
                await _db.UpdateAsync(release);
            }

            var allArtists = await _db.Table<Database.Entities.Artist>().ToListAsync();
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

            var artistToTagsTable = await _db.Table<MusicBrainzArtistToMusicBrainzTags>().ToListAsync();
            var artistsToTagsToRemove = artistToTagsTable.Where(x => x.MusicBrainzArtistId == badMusicBrainzArtistId).ToList();
            foreach (var artistToTag in artistsToTagsToRemove)
            {
                await _db.DeleteAsync(artistToTag);
            }

            var artistToReleaseTable = await _db.Table<MusicBrainzArtistToMusicBrainzRelease>().ToListAsync();
            var artistsToReleasesToRemove = artistToReleaseTable.Where(x => x.MusicBrainzArtistId == badMusicBrainzArtistId).ToList();
            var listOfReleaseIdsTiedToThisArtist = new List<string>();
            foreach (var artistRecord in artistsToReleasesToRemove)
            {
                listOfReleaseIdsTiedToThisArtist.Add(artistRecord.MusicBrainzReleaseId);
                await _db.DeleteAsync(artistRecord);
            }


            var coverImageTable = await _db.Table<MusicBrainzReleaseToCoverImage>().ToListAsync();
            var coverImagesForThisRelease = thisReleaseList.FirstOrDefault().MusicBrainzReleaseId;
            var coverImagesStoredForBadReleases = coverImageTable.Where(x => listOfReleaseIdsTiedToThisArtist.Contains(x.MusicBrainzReleaseId)).ToList();
            foreach (var coverImageRecord in coverImagesStoredForBadReleases)
            {
                await _db.DeleteAsync(coverImageRecord);
            }
        }
    }

}
