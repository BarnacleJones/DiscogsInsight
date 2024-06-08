using DiscogsInsight.ApiIntegration.Contract;
using DiscogsInsight.ApiIntegration.Models.DiscogsResponseModels;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.Database.Entities;
using DiscogsInsight.Database.Contract;
using Microsoft.Extensions.Logging;
using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.ApiIntegration.Models.MusicBrainzResponseModels;
using System.Reflection.Metadata;
using Release = DiscogsInsight.Database.Entities.Release;
using Track = DiscogsInsight.Database.Entities.Track;
using Artist = DiscogsInsight.Database.Entities.Artist;

namespace DiscogsInsight.DataAccess.Services
{
    public class ReleaseDataService : IReleaseDataService
    {
        private readonly IDiscogsInsightDb _dbService;
        private readonly ISQLiteAsyncConnection _db;
        private readonly IMusicBrainzApiService _musicBrainzApiService;
        private readonly IDiscogsApiService _discogsApiService;
        private readonly ICoverArtArchiveApiService _coverArchiveApiService;
        private readonly ICollectionDataService _collectionDataService;
        private readonly IArtistDataService _artistDataService;
        private readonly ITracksDataService _tracksDataService;
        private readonly ILogger<ReleaseDataService> _logger;
        private readonly IDiscogsGenresAndTagsDataService _discogsGenresAndTagsDataService;

        public ReleaseDataService(IDiscogsInsightDb dbService, ISQLiteAsyncConnection db, IMusicBrainzApiService musicBrainzApiService, IDiscogsGenresAndTagsDataService discogsGenresAndTags, IArtistDataService artistDataService, IDiscogsApiService discogsApiService, ICollectionDataService collectionDataService, ICoverArtArchiveApiService coverArchiveApiService, ITracksDataService tracksDataService, ILogger<ReleaseDataService> logger)
        {
            _dbService = dbService;
            _db = db;
            _musicBrainzApiService = musicBrainzApiService;
            _collectionDataService = collectionDataService;
            _artistDataService = artistDataService;
            _coverArchiveApiService = coverArchiveApiService;
            _discogsApiService = discogsApiService;
            _discogsGenresAndTagsDataService = discogsGenresAndTags;
            _logger = logger;
            _tracksDataService = tracksDataService;
        }

        public async Task<bool> SetFavouriteBooleanOnRelease(bool favourited, int discogsReleaseId)
        {
            try
            {
                var thisRelease = await _db.Table<Release>().FirstOrDefaultAsync(x => x.DiscogsReleaseId == discogsReleaseId);
                thisRelease.IsFavourited = favourited;

                await _db.UpdateAsync(thisRelease);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<Database.Entities.Release>> GetAllReleasesAsList()
        {
            var releases = await _db.Table<Release>().ToListAsync();
            return releases;
        }

        public async Task<List<int?>> GetAllDiscogsReleaseIdsForArtist(int? discogsArtistId)
        {
            if (discogsArtistId == null) { return new List<int?>(); }

            var discogsReleaseIdsForArtistQuery = $@"
                SELECT DiscogsArtistId
                FROM Release
                WHERE DiscogsArtistId = {discogsArtistId};";

            var discogsReleaseIdsForArtist = await _db.QueryAsync<int?>(discogsReleaseIdsForArtistQuery, new { discogsArtistId });
            return discogsReleaseIdsForArtist;

        }

        public async Task<(Database.Entities.Release?, byte[]?)> GetReleaseAndImageAndRetrieveAllApiDataForRelease(int? discogsReleaseId)
        {
            if (discogsReleaseId == null)
                throw new Exception($"Missing discogs release id");

            var release = await _db.Table<Database.Entities.Release>().FirstOrDefaultAsync(x => x.DiscogsReleaseId == discogsReleaseId);

            if (release == null || !release.HasAllApiData)
            {
                await RetrieveAllApiDataForReleaseAndReleasesArtist(discogsReleaseId.Value);
                //requery
                release = await _db.Table<Database.Entities.Release>().FirstOrDefaultAsync(x => x.DiscogsReleaseId == discogsReleaseId);
            }
            ///////
            //keeping this here for if there are errros. i dont know why id be checking if release is null when ive retrieved it in the line above
            ////if release is null save it and the tracks here
            //if (release == null)
            //{
            //    var releases = await _collectionDataService.GetReleases();
            //    release = releases.FirstOrDefault(x => x.DiscogsReleaseId == discogsReleaseId);

            //}
            ////////
            var coverImage = await GetImageForRelease(release.MusicBrainzReleaseId);

            return (release, coverImage);
        }
            

        public async Task<byte[]?> GetImageForRelease(string musicBrainzReleaseId)
        {
            if (string.IsNullOrEmpty(musicBrainzReleaseId))
                return [];
            
            var record =  await _db.Table<MusicBrainzReleaseToCoverImage>().Where(x => x.MusicBrainzReleaseId == musicBrainzReleaseId).FirstOrDefaultAsync();
            return record.MusicBrainzCoverImage;
           
        }

        public class TestClass
        {
            public Blob MusicBrainzCoverImage { get; set; }
        }

        public async Task<(Database.Entities.Release?, byte[]?)> GetRandomRelease()//not doing any checks for cover art/musicbrainz data - todo fix this
        {
            var releases = await _db.Table<Database.Entities.Release>().ToListAsync();
            if (releases.Count < 1)
            {
                releases = await _collectionDataService.GetReleases();
            }

            var randomRelease = releases.Select(x => x.DiscogsReleaseId).OrderBy(r => Guid.NewGuid()).FirstOrDefault();//new GUID as key, will be random

            if (randomRelease is null)
            {
                throw new Exception($"Error getting random release.");
            }

            var release = await GetReleaseAndImageAndRetrieveAllApiDataForRelease(randomRelease);//get additional release info from apis if they dont exist

            return (release.Item1, release.Item2);
        }

        public async Task<List<Database.Entities.Release>> GetNewestReleases(int howManyToReturn)
        {
            var returnedReleases = new List<Release>();

            var releases = await _db.Table<Release>().ToListAsync();

            if (releases.Count < 1)//seed the collection if no releases
            {
                releases = await _collectionDataService.GetReleases();
            }
            var moreReleasesThanHowManyToReturn = releases.Count() >= howManyToReturn;

            var latestXReleases = releases.OrderByDescending(r => r.DateAdded)
                           .Take(moreReleasesThanHowManyToReturn ? howManyToReturn : 1)
                           .ToList();

            foreach (var release in latestXReleases)
            {
                var releaseToAdd = await GetReleaseAndImageAndRetrieveAllApiDataForRelease(release.DiscogsReleaseId);
                returnedReleases.Add(releaseToAdd.Item1 ?? new Release());//retrieves extra info
            }
            return returnedReleases;
        }

        #region Private Methods
        private async Task MakeMusicBrainzReleaseCallAndSaveTracks(Database.Entities.Release release, string? musicBrainzReleaseId, bool isAReleaseGroupUrl)
        {
            if (!isAReleaseGroupUrl)
            {
                //save tracks and release year
                var releaseData = await _musicBrainzApiService.GetReleaseFromMusicBrainzApiUsingMusicBrainsReleaseId(musicBrainzReleaseId);
                release.OriginalReleaseYear = releaseData.Date;
                await _db.UpdateAsync(release);

                var tracksFromReleaseData = releaseData.Media.SelectMany(x => x.Tracks).ToList();
                if (!tracksFromReleaseData.Any()) { return; }

                var tracksForThisRelease = await _db.Table<Database.Entities.Track>()
                                                    .Where(x => x.DiscogsReleaseId == release.DiscogsReleaseId)
                                                    .ToListAsync();   
                
                foreach (var track in tracksForThisRelease)
                {
                    var levenshteinDistanceAndTrackLength = new List<(int, int?)>();
                    foreach (var apiTrack in tracksFromReleaseData)
                    {
                        //compare each track name in response to track name of album
                        //store the Levenshtein Distance and the length from api
                        int levenshteinDistance = Fastenshtein.Levenshtein.Distance(apiTrack.Title, track.Title);
                        levenshteinDistanceAndTrackLength.Add((levenshteinDistance, apiTrack.Length));
                    }
                    //sort by distance - lowest number of edits is the most similar
                    if (levenshteinDistanceAndTrackLength.Any())
                    {
                        levenshteinDistanceAndTrackLength.Sort((x, y) => x.Item1.CompareTo(y.Item1));
                        var matchingRelease = levenshteinDistanceAndTrackLength.First();

                        track.MusicBrainzTrackLength = matchingRelease.Item2;
                        await _db.UpdateAsync(track);
                    }
                }
            }
            else
            {
                //just save year
                var releaseGroupData = await _musicBrainzApiService.GetReleaseGroupFromMusicBrainzApiUsingMusicBrainsReleaseId(musicBrainzReleaseId);
                release.OriginalReleaseYear = releaseGroupData.FirstReleaseDate;
                await _db.UpdateAsync(release);
            }
        }
       
       
        private async Task SaveReleasesFromMusicBrainzArtistCall(string musicBrainzArtistId, int? discogsArtistId)
        {
            if (string.IsNullOrWhiteSpace(musicBrainzArtistId))
            {
                return;
            }
            var artistCallResponse = await _musicBrainzApiService.GetArtistFromMusicBrainzApiUsingArtistId(musicBrainzArtistId);
            var releasesByArtist = artistCallResponse.Releases?.ToList();
            var releaseGroupsByArtist = artistCallResponse.ReleaseGroups?.ToList();
         
            var existingMusicBrainzReleaseIdIdsForThisArtistQuery = @$"
                SELECT MusicBrainzReleaseId
                FROM MusicBrainzArtistToMusicBrainzRelease
                WHERE MusicBrainzArtistToMusicBrainzRelease.MusicBrainzArtistId = {musicBrainzArtistId};";

            var existingMusicBrainzReleaseIdsForThisArtist = await _db.QueryAsync<MusicBrainzReleaseIdResponse>(existingMusicBrainzReleaseIdIdsForThisArtistQuery, new { musicBrainzArtistId });
            var existingMusicBrainzReleaseIdsForThisArtistAsStringList = existingMusicBrainzReleaseIdsForThisArtist.Select(x => x.MusicBrainzReleaseId).ToList();
           
            if (releasesByArtist != null && releasesByArtist.Any())
            {
                foreach (var artistsRelease in releasesByArtist)
                {
                    if (existingMusicBrainzReleaseIdsForThisArtistAsStringList.Contains(artistsRelease.Id))
                    {
                        continue;//already exists
                    }
                    var artistIdToReleaseId = new MusicBrainzArtistToMusicBrainzRelease
                    {
                        MusicBrainzArtistId = musicBrainzArtistId,
                        DiscogsArtistId = discogsArtistId ?? 0,//should never get 0's here
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
                    if (existingMusicBrainzReleaseIdsForThisArtistAsStringList.Contains(artistsReleaseGroup.Id))
                    {
                        continue;//already exists
                    }
                    var artistIdToReleaseId = new MusicBrainzArtistToMusicBrainzRelease
                    {
                        MusicBrainzArtistId = musicBrainzArtistId,
                        DiscogsArtistId = discogsArtistId ?? 0,//should never get 0's here
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
            
        
        private async Task<MusicBrainzArtistToMusicBrainzRelease?> GetMusicBrainzReleaseIdFromDiscogsReleaseInformation(string? discogsTitle, int discogsArtistId)
        {
        //using Fastenshtein.Levenshtein.Distance algorithm https://github.com/DanHarltey/Fastenshtein
            //to get the most similar musicbrainz title based on the discogs title
            var releaseJoiningListByDiscogsArtist = await _db.Table<MusicBrainzArtistToMusicBrainzRelease>().Where(x => x.DiscogsArtistId == discogsArtistId).ToListAsync();

            var levenshteinDistanceAndReleaseIds = new List<(int, MusicBrainzArtistToMusicBrainzRelease)>();

            foreach (var release in releaseJoiningListByDiscogsArtist)
            {
                //compare each release name in joining table to discogs title
                //store the Levenshtein Distance 
                int levenshteinDistance = Fastenshtein.Levenshtein.Distance(release.MusicBrainzReleaseName, discogsTitle);
                levenshteinDistanceAndReleaseIds.Add((levenshteinDistance, release));
            }
            //sort by distance - lowest number of edits is the most similar
            if (levenshteinDistanceAndReleaseIds.Any())
            {
                levenshteinDistanceAndReleaseIds.Sort((x, y) => x.Item1.CompareTo(y.Item1));
                var matchingRelease = levenshteinDistanceAndReleaseIds.First();
                return matchingRelease.Item2;
            }
            return null;
        }


        #endregion

        #region Release Data Correction

        public async Task<List<PossibleReleasesFromArtist>> GetPossibleReleasesForDataCorrectionFromDiscogsReleaseId(int? discogsReleaseId)
        {
            var allReleasesKnownByArtistId = await GetAllStoredMusicBrainzReleasesForArtistByDiscogsReleaseId(discogsReleaseId);

            //remove existing cover image
            var imagesToRemove = await _db.Table<MusicBrainzReleaseToCoverImage>().Where(x => x.MusicBrainzReleaseId == allReleasesKnownByArtistId.Item1).ToListAsync();

            foreach (var image in imagesToRemove)
            {
                await _db.DeleteAsync(image);
            }

            return allReleasesKnownByArtistId.Item2;
        }

        public async Task<(string, List<PossibleReleasesFromArtist>)> GetAllStoredMusicBrainzReleasesForArtistByDiscogsReleaseId(int? discogsReleaseId)
        {
            var incorrectReleases = await _db.Table<Release>().Where(x => x.DiscogsReleaseId == discogsReleaseId).ToListAsync();

            var discogsArtistId = incorrectReleases.FirstOrDefault().DiscogsArtistId;
            var badMusicBrainzReleaseId = incorrectReleases.FirstOrDefault().MusicBrainzReleaseId ?? "";

            var artistList = await _db.Table<Artist>().Where(x => x.DiscogsArtistId == discogsArtistId).FirstOrDefaultAsync();
            var musicBrainzArtistId = artistList.MusicBrainzArtistId;

            var releaseJoiningListByMusicBrainzArtist = await _db.Table<MusicBrainzArtistToMusicBrainzRelease>().Where(x => x.MusicBrainzArtistId == musicBrainzArtistId).ToListAsync();

            var allReleasesKnownByArtistId = releaseJoiningListByMusicBrainzArtist.Select(x => new PossibleReleasesFromArtist
            {
                Date = x.ReleaseYear ?? "",
                MusicBrainzReleaseId = x.MusicBrainzReleaseId ?? "",
                Status = x.Status ?? "",
                Title = x.MusicBrainzReleaseName ?? ""
            }).ToList();
            return (badMusicBrainzReleaseId, allReleasesKnownByArtistId);
        }

        public async Task<bool> UpdateReleaseToBeNewMusicBrainzReleaseId(int? discogsReleaseId, string musicBrainzReleaseId)
        {
            var releaseTable = await GetAllReleasesAsList();
            var releasesToChange = releaseTable.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();
            var newRelease = await _db.Table<MusicBrainzArtistToMusicBrainzRelease>().Where(x => x.MusicBrainzReleaseId == musicBrainzReleaseId).FirstOrDefaultAsync();

            foreach (var release in releasesToChange)
            {
                release.MusicBrainzReleaseId = musicBrainzReleaseId;
                release.ReleaseHasBeenManuallyCorrected = true;
                release.HasAllApiData = false;
                release.IsAReleaseGroupGroupId = newRelease.IsAReleaseGroupGroupId;
                await _db.UpdateAsync(release);
            }

            return true;
        }
        #endregion

        #region New style/checked or refactored methods
        public async Task<List<FullReleaseDataModel>> GetReleaseDataModelsByDiscogsGenreTagId(int discogsGenreTagId)
        {
            var releasesWithThisGenreIdQuery =$@"
                SELECT DiscogsReleaseId
                FROM DiscogsGenreTagToDiscogsRelease
                WHERE DiscogsGenreTagId = 1;";

            var discogsReleaseIds = await _db.QueryAsync<DiscogsReleaseIdClass>(releasesWithThisGenreIdQuery, new { discogsGenreTagId });
                   
            var releasesByGenreQuery =@$"
                SELECT 
                Release.Year,
                Release.OriginalReleaseYear, 
                Release.Title,
                Artist.Name as Artist,
                Release.ReleaseNotes,
                Release.ReleaseCountry,
                Release.DiscogsArtistId,
                Release.DiscogsReleaseId,
                Release.MusicBrainzReleaseId,
                Release.DiscogsReleaseUrl,
                Release.DateAdded,
                Release.IsFavourited
                Release.HasAllApiData
                FROM Release
                INNER JOIN Artist on Release.DiscogsArtistId = Artist.DiscogsArtistId
                WHERE Release.DiscogsReleaseId in ({string.Join(",", discogsReleaseIds)});";

            var releasesByGenre = await _db.QueryAsync<ReleaseInterimData>(releasesWithThisGenreIdQuery, new { discogsGenreTagId });

            //First fetch all API data if there is any missing
            var releasesByGenreWithoutAllApiData = releasesByGenre.Where(x => !x.HasAllApiData && x.DiscogsReleaseId.HasValue).Select(x => x.DiscogsReleaseId);
            if (releasesByGenreWithoutAllApiData.Any())
            {
                await GetAllApiDataForListOfDiscogsReleaseIds(releasesByGenreWithoutAllApiData);
                //requery
                releasesByGenre = await _db.QueryAsync<ReleaseInterimData>(releasesWithThisGenreIdQuery, new { discogsGenreTagId });

            }

            var returnedReleases = new List<FullReleaseDataModel>();
            foreach (var item in releasesByGenre)
            {                

                var tracksForListRelease = await _db.Table<Database.Entities.Track>().Where(x => x.DiscogsReleaseId == item.DiscogsReleaseId).ToListAsync();
                var artist = await _db.Table<Database.Entities.Artist>().Where(x => x.DiscogsArtistId == item.DiscogsArtistId).FirstOrDefaultAsync();
                var image = await GetImageForRelease(item.MusicBrainzReleaseId);
                                
                returnedReleases.Add(await GetReleaseDataModel(item, tracksForListRelease, artist.Name, image));
            }

            return returnedReleases;
        }

        public async Task GetAllApiDataForListOfDiscogsReleaseIds(IEnumerable<int?> releasesWhereHasAllApiDataIsFalse)
        {
            foreach (var release in releasesWhereHasAllApiDataIsFalse)
            {
                if (release is null) continue;
                await RetrieveAllApiDataForReleaseAndReleasesArtist(release.Value);
            }
        }

        //see if i can swap classes out for objects once things are running
        public class DiscogsReleaseIdClass
        {
            public int DiscogsReleaseId { get; set; }
        }
        public class DiscogsArtistIdClass
        {
            public int DiscogsArtistId { get; set; }
        }

        public class TagDbResponse
        {
            public string Tag { get; set; }
        }
        public class MusicBrainzReleaseIdResponse
        {
            public string MusicBrainzReleaseId { get; set; }
        }

        public class ReleaseInterimData
        {
            public string? Year { get; set; }
            public string? OriginalReleaseYear { get; set; }
            public string? Title { get; set; }
            public string? Artist { get; set; }
            public string? ReleaseNotes { get; set; }
            public string? ReleaseCountry { get; set; }
            public int? DiscogsArtistId { get; set; }
            public int? DiscogsReleaseId { get; set; }
            public string MusicBrainzReleaseId { get; set; }
            //public List<(string? Name, int Id)>? Genres { get; set; }
            public string? DiscogsReleaseUrl { get; set; }
            public DateTime? DateAdded { get; set; }
            //public List<TrackDto>? Tracks { get; set; }
            //public byte[] CoverImage { get; set; }
            public bool IsFavourited { get; set; }
            public bool HasAllApiData { get; set; }
        }

        private Task<Database.Entities.Release> GetReleaseByDiscogsReleaseId(int discogsReleaseId)
        {
            var release = _db.Table<Database.Entities.Release>().FirstOrDefaultAsync(x => x.DiscogsReleaseId == discogsReleaseId);
            if (release is null) throw new Exception("No release - try refreshing data ????");
            return release;
        }

        /// <summary>
        /// Saves Tracks, Genres, Country, Notes, and Release Url
        /// </summary>
        /// <param name="existingRelease"></param>
        /// <param name="releaseResponse"></param>
        /// <returns></returns>
        private async Task SaveInformationFromDiscogsReleaseResponse(Database.Entities.Release existingRelease, DiscogsReleaseResponse releaseResponse)
        {
            var tracksAlreadySavedForThisRelease = await _db.Table<Database.Entities.Track>().Where(x => x.DiscogsReleaseId == existingRelease.DiscogsReleaseId).CountAsync() > 0;
                
            await UpdateAdditionalReleaseProperties(releaseResponse, existingRelease);

            if (!tracksAlreadySavedForThisRelease && releaseResponse.tracklist != null)
            {
                foreach (var track in releaseResponse.tracklist)
                {
                    await _db.InsertAsync(new Database.Entities.Track
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
                
            //save genres (styles) from response if there are any
            if (releaseResponse.styles == null || !releaseResponse.styles.Any())
                return;

            var releaseGenresFromReleaseResponse = releaseResponse.styles;
            //todo have a think about how to further optimise this
            //when coming from all releases by genre - still getting the whole table per release
            //not as bad if just loading a single release
            var genreInDatabaseAlready = await _db.Table<DiscogsGenreTags>().ToListAsync();
            var genresNotInDatabaseAlready = releaseGenresFromReleaseResponse.Except(genreInDatabaseAlready.Select(y => y.DiscogsTag)).ToList();

            bool needToRequeryGenreTable = false;

            //save to genre table 
            if (genresNotInDatabaseAlready.Any())
            {
                needToRequeryGenreTable = true;
                foreach (var genreName in genresNotInDatabaseAlready)
                {
                    await _db.InsertAsync(new DiscogsGenreTags { DiscogsTag = genreName });
                }                    
            }

            if (needToRequeryGenreTable)
            {
                genreInDatabaseAlready = await _db.Table<DiscogsGenreTags>().ToListAsync();
            }

            //save to genre/release joining table
            foreach (var style in releaseGenresFromReleaseResponse)
            {
                var genreTagId = genreInDatabaseAlready.Where(x => x.DiscogsTag == style).Select(x => x.Id).FirstOrDefault();

                await _db.InsertAsync(new DiscogsGenreTagToDiscogsRelease
                {
                    DiscogsReleaseId = existingRelease.DiscogsReleaseId,
                    DiscogsArtistId = existingRelease.DiscogsArtistId,
                    DiscogsGenreTagId = genreTagId
                });
            }          
        }
        private async Task UpdateAdditionalReleaseProperties(DiscogsReleaseResponse releaseResponse, Database.Entities.Release existingRelease)
        {
            //update existing release entity with additional properties
            existingRelease.ReleaseCountry = releaseResponse.country;
            existingRelease.ReleaseNotes = releaseResponse.notes;
            existingRelease.DiscogsReleaseUrl = releaseResponse.uri;
            await _db.UpdateAsync(existingRelease);
        }
        private async Task RetrieveAllApiDataForReleaseAndReleasesArtist(int discogsReleaseId)
        {

            var release = await GetReleaseByDiscogsReleaseId(discogsReleaseId);
            //release may have been updated from bad data (which corrects musicbrainz data)
            //so there is potential the release doesnt have all data, but dont want to refetch the discogs data - as that is baseline data that doesnt change in correction
            //todo unless discogs page has updated, which one day I will look into for updating data
            //so doing a check on one of the additional release data fields (Release DiscogsReleaseUrl - which is the most likely to be populated)
            //from the test with a collection size 450 the only release that has all api data but no release url is a Dolly Parton (9-5) 45 inch...this is flawed but seems rareish
            if (string.IsNullOrWhiteSpace(release.DiscogsReleaseUrl))
            {
                var discogsReleaseResponse = await _discogsApiService.GetReleaseFromDiscogs((int)discogsReleaseId);
                await SaveInformationFromDiscogsReleaseResponse(release, discogsReleaseResponse);
            }
           
            var artist = await _db.Table<Database.Entities.Artist>().FirstOrDefaultAsync(x => x.DiscogsArtistId == release.DiscogsArtistId);

            if (artist == null)
                throw new Exception("No artist - try refreshing data ????");

            //this is the first time the fuzzy logic comes in to play, so skip this for data thats been manually corrected, as all that release data has been refetched
            if (release.MusicBrainzReleaseId == null && !release.ArtistHasBeenManuallyCorrected)
            {
                //dont fetch api data for 'Various' artists, it 404s for discogs, and causes bad data with MusicBrainz. 
                if (artist.Name?.ToLower() != "various" && !artist.HasAllApiData)
                {
                    if (string.IsNullOrWhiteSpace(artist.Profile) && artist.DiscogsArtistId.HasValue)
                    {
                        var discogsResult = await _discogsApiService.GetArtistFromDiscogs(artist.DiscogsArtistId.Value);
                        //Add additional properties wanted from Artist Discogs call here...profile is really the only useful one here
                        artist.Profile = discogsResult.profile;
                    }
                    if (artist.MusicBrainzArtistId == null && artist.Name != null)
                    {
                        var musicBrainzResult = await _musicBrainzApiService.GetInitialArtistFromMusicBrainzApi(artist.Name);
                        await SetMusicBrainzArtistDataForSavingAndSaveTagsFromArtistResponse(musicBrainzResult, artist);
                    }
                }
                artist.HasAllApiData = true;
                await _db.UpdateAsync(artist);

                await SaveReleasesFromMusicBrainzArtistCall(artist.MusicBrainzArtistId, artist.DiscogsArtistId);
            }
            //using the artist id need to get releases and figure out which one is the right release
            //potential here for the release not being right.

            //The release may not exist on that call, this is using Levenshtein algorithm which is not right every time.

            if (release.ArtistHasBeenManuallyCorrected)//coming into this function the first time after correcting artist data
            {
                var mostLikelyRelease = await GetMusicBrainzReleaseIdFromDiscogsReleaseInformation(release.Title, release.DiscogsArtistId ?? 0);

                if (mostLikelyRelease != null)
                {
                    release.IsAReleaseGroupGroupId = mostLikelyRelease.IsAReleaseGroupGroupId;
                    release.MusicBrainzReleaseId = mostLikelyRelease.MusicBrainzReleaseId;
                    await _db.UpdateAsync(release);
                }
                //refetch
                release = await GetReleaseByDiscogsReleaseId(discogsReleaseId);
            }

            //release having the data corrected will fall here
            if (release.MusicBrainzReleaseId != null || release.ArtistHasBeenManuallyCorrected)
            {
                //now make the musicbrainz release call with the release id and get all the track lengths (if not a releasegroup) and original year
                await MakeMusicBrainzReleaseCallAndSaveTracks(release, release.MusicBrainzReleaseId, release.IsAReleaseGroupGroupId);

            }
            var coverImageExists = await _db.Table<MusicBrainzReleaseToCoverImage>().Where(x => x.MusicBrainzReleaseId == release.MusicBrainzReleaseId).CountAsync() == 0;
            
            if (!coverImageExists && release?.MusicBrainzReleaseId != null)//various artist albums will not get a release id or cover image
            {
                var coverApiResponse = await _coverArchiveApiService.GetCoverResponseByMusicBrainzReleaseId(release.MusicBrainzReleaseId, release.IsAReleaseGroupGroupId);
                if (coverApiResponse != null)
                {                    
                    var thumbnailUrls = coverApiResponse.Images.Select(x => x.Thumbnails).ToList();
                    var coverUrl = thumbnailUrls.Select(x => x._500 ?? x.Small).FirstOrDefault();//choosing to save 500 or small one, can go bigger or larger 

                    release.MusicBrainzCoverUrl = coverUrl;

                    var coverByteArray = await _coverArchiveApiService.GetCoverByteArray(coverUrl);

                    var releaseToCoverImage = new MusicBrainzReleaseToCoverImage
                    {
                        MusicBrainzReleaseId = release.MusicBrainzReleaseId,
                        MusicBrainzCoverImage = coverByteArray
                    };
                    await _db.InsertAsync(releaseToCoverImage);
                }
            }
            release.HasAllApiData = true;//all api data retrieved for release
            release.ArtistHasBeenManuallyCorrected = false;//change back as all data is done until next correction
            release.ReleaseHasBeenManuallyCorrected = false;//change back as all data is done until next correction
            await _db.UpdateAsync(release);
        }

        /// <summary>
        /// Updates Artist Fields (but doesnt update artist record) and saves any tags not in db and saves tags from response to MusicBrainzArtistToMusicBrainzTags joining table
        /// </summary>
        /// <param name="artistResponse"></param>
        /// <param name="existingArtist"></param>
        /// <returns></returns>
        private async Task SetMusicBrainzArtistDataForSavingAndSaveTagsFromArtistResponse(MusicBrainzInitialArtist artistResponse, Database.Entities.Artist existingArtist)
        {
            //**this makes an assumption that can cause bad data**
            //Artists in the response is a list, there are similar named artists in the list
            //It looks like the first in the list is closest match
            //Ability to correct this data is on the release card

            var musicBrainsArtistDataToSave = artistResponse.Artists.Select(x => new 
            {
                x.Id,
                x.Area,
                BeginAreaName = x.BeginArea?.Name,
                BeginAreaType = x.BeginArea?.Type,
                AreaName = x.Area?.Name,
                AreaType = x.Area?.Type,
                x.LifeSpan?.Begin,
                x.LifeSpan?.End,
                x.Tags
            }).FirstOrDefault();

            existingArtist.MusicBrainzArtistId = musicBrainsArtistDataToSave.Id;

            var beginAreaName = musicBrainsArtistDataToSave.BeginAreaName;
            if (beginAreaName != null)
            {
                if (musicBrainsArtistDataToSave.BeginAreaType == "City")
                {
                    existingArtist.City = beginAreaName;
                }
                else
                {
                    existingArtist.Country = beginAreaName;
                }
            }

            var areaName = musicBrainsArtistDataToSave.AreaName;
            if (areaName != null)
            {
                if (musicBrainsArtistDataToSave.AreaType == "City")
                {
                    existingArtist.City = areaName;
                }
                else
                {
                    existingArtist.Country = areaName;
                }
            }

            existingArtist.StartYear = musicBrainsArtistDataToSave.Begin;
            existingArtist.EndYear = musicBrainsArtistDataToSave.End;

            //save tags - yes it is saving to joining table too before saving the actual artist table information, but its saving calls to the db....
            var tagsNamesInResponse = musicBrainsArtistDataToSave.Tags.Where(x => x.Count > 1).Select(x => x.Name);//excludes a lot of random tags

            if (tagsNamesInResponse.Any())
            {
                var musicBrainsTagRecordsForGivenTagsDbQuery = @$"
                                                        SELECT Id, Tag
                                                        FROM MusicBrainzTags
                                                        WHERE Tag IN ({string.Join(",", tagsNamesInResponse)})`;
                                                        ";
                var reponseTagNamesAlreadyInDbClassObject = await _db.QueryAsync<MusicBrainzTags>(musicBrainsTagRecordsForGivenTagsDbQuery, new { tagsNamesInResponse });
                var reponseTagNamesAlreadyInDb = reponseTagNamesAlreadyInDbClassObject.Select(x => x.Tag).ToList();
                var tagNamesToSave = tagsNamesInResponse.Except(reponseTagNamesAlreadyInDb).ToList();
                
                if (tagNamesToSave.Any())
                {
                    foreach (var tag in tagNamesToSave)
                    {
                        var tagToSave = new MusicBrainzTags { Tag = tag };
                        await _db.InsertAsync(tagToSave);
                    }
                    //requery                    
                    reponseTagNamesAlreadyInDbClassObject = await _db.QueryAsync<MusicBrainzTags>(musicBrainsTagRecordsForGivenTagsDbQuery, new { tagsNamesInResponse });
                }
                foreach (var tag in tagsNamesInResponse)
                {
                    var tagId = reponseTagNamesAlreadyInDbClassObject.Where(x => x.Tag == tag).FirstOrDefault()?.Id;
                    if (tagId != null)
                    {
                        var tagToArtist = new MusicBrainzArtistToMusicBrainzTags { TagId = tagId.Value, MusicBrainzArtistId = existingArtist.MusicBrainzArtistId };
                        await _db.InsertAsync(tagToArtist);
                    }
                }
            }

        }
        private async Task<FullReleaseDataModel> GetReleaseDataModel(ReleaseInterimData release, List<Database.Entities.Track> trackList, string? releaseArtistName, byte[]? imageAsBytes)
        {
            var trackListAsViewModel = trackList.Select(x => new TrackDto
            {
                Duration = x.MusicBrainzTrackLength == null
                                ? x.Duration
                                : TimeSpan.FromMilliseconds(x.MusicBrainzTrackLength.Value).ToString(@"mm\:ss"),
                Position = x.Position,
                Title = x.Title,
                Rating = x.Rating ?? 0,
                DiscogsArtistId = x.DiscogsArtistId ?? 0,
                DiscogsReleaseId = x.DiscogsReleaseId ?? 0
            }).ToList();

            var genres = await _discogsGenresAndTagsDataService.GetGenresForDiscogsRelease(release.DiscogsReleaseId);

            return new FullReleaseDataModel
            {
                Artist = releaseArtistName ?? "Missing Artist",
                Year = release.Year.ToString(),
                OriginalReleaseYear = release.OriginalReleaseYear,
                ReleaseCountry = release.ReleaseCountry,
                Title = release.Title ?? "Missing Title",
                ReleaseNotes = release.ReleaseNotes ?? "",
                Genres = genres,
                DiscogsReleaseUrl = release.DiscogsReleaseUrl,
                Tracks = trackListAsViewModel,
                DateAdded = release.DateAdded,
                DiscogsArtistId = release.DiscogsArtistId,
                DiscogsReleaseId = release.DiscogsReleaseId,
                CoverImage = imageAsBytes ?? [],
                IsFavourited = release.IsFavourited,
            };
        }

        #endregion



    }
}
