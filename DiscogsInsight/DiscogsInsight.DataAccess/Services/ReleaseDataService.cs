using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using DiscogsInsight.ApiIntegration.Services;
using DiscogsInsight.DataAccess.Entities;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.DataAccess.Services
{
    public class ReleaseDataService
    {
        private readonly DiscogsInsightDb _db;
        private readonly MusicBrainzApiService _musicBrainzApiService;
        private readonly DiscogsApiService _discogsApiService;
        private readonly CoverArtArchiveApiService _coverArchiveApiService;
        private readonly CollectionDataService _collectionDataService;
        private readonly ArtistDataService _artistDataService;
        private readonly ILogger<ReleaseDataService> _logger;
        private readonly DiscogsGenresAndTagsDataService _discogsGenresAndTagsDataService;

        public ReleaseDataService(DiscogsInsightDb db, MusicBrainzApiService musicBrainzApiService, DiscogsGenresAndTagsDataService discogsGenresAndTags, ArtistDataService artistDataService, DiscogsApiService discogsApiService, CollectionDataService collectionDataService, CoverArtArchiveApiService coverArchiveApiService, ILogger<ReleaseDataService> logger)
        {
            _db = db;
            _musicBrainzApiService = musicBrainzApiService;
            _collectionDataService = collectionDataService;
            _artistDataService = artistDataService;
            _coverArchiveApiService = coverArchiveApiService;
            _discogsApiService = discogsApiService;
            _discogsGenresAndTagsDataService = discogsGenresAndTags;
            _logger = logger;
        }

        public async Task<bool> SetFavouriteBooleanOnRelease(bool favourited, int discogsReleaseId)
        {
            try
            {
                var release = await _db.GetAllEntitiesAsListAsync<Release>();

                var thisRelease = release.Where(x => x.DiscogsReleaseId == discogsReleaseId).FirstOrDefault();

                thisRelease.IsFavourited = favourited;

                await _db.SaveItemAsync(thisRelease);

                return true;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public async Task<List<Release>> GetAllReleasesAsList()
        {
            var releases = await _db.GetAllEntitiesAsListAsync<Release>();
            return releases;
        }

        public async Task<List<int?>> GetAllDiscogsReleaseIdsForArtist(int? discogsArtistId)
        {
            if (discogsArtistId == null) { return new List<int?>(); }

            var releases = await _db.GetAllEntitiesAsListAsync<Release>();
            var releaseList = releases.ToList();

            return releaseList.Where(x => x.DiscogsArtistId == discogsArtistId)
                              .Select(x => x.DiscogsReleaseId)
                              .ToList();
        }

        //Todo this is getting too big - make mini functions
        public async Task<(Release?, byte[]?)> GetReleaseAndImageAndRetrieveAllApiDataForRelease(int? discogsReleaseId)
        {
            if (discogsReleaseId == null)
                throw new Exception($"Missing discogs release id");

            var release = await GetReleaseFromDbByDiscogsReleaseId(discogsReleaseId.Value);

            //if release is null save it and the tracks here
            if (release == null)
            {
                var releases = await _collectionDataService.GetReleases();
                release = releases.FirstOrDefault(x => x.DiscogsReleaseId == discogsReleaseId);

            }
            //release may have been updated from bad data so doesnt have all data, but dont want the discogs data
            //so doing a check on one of the additional release data fields (Release DiscogsReleaseUrl - which is likely to be populated)
            if (!release.HasAllApiData && string.IsNullOrWhiteSpace(release.DiscogsReleaseUrl))
            {
                var discogsReleaseResponse = await _discogsApiService.GetReleaseFromDiscogs((int)discogsReleaseId);
                var success = await SaveTracksAndAdditionalInformationFromDiscogsReleaseResponse(discogsReleaseResponse);
                if (!success)
                {
                    throw new Exception("Error saving tracklist to database");
                }
                var tracks = await _db.GetAllEntitiesAsListAsync<Track>();
                var trackList = tracks.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();
            }
            var artistsTable = await _db.GetAllEntitiesAsListAsync<Artist>();
            var artists = artistsTable.ToList();
            var artist = artists.FirstOrDefault(x => x.DiscogsArtistId == release.DiscogsArtistId);

            if (artist == null)
                throw new Exception("No artist - try refreshing data ????");
            if (release == null)
                throw new Exception("No release - try refreshing data ????");

            //this is the first time the fuzzy logic comes in to play, so skip this for data thats been manually corrected, as all that release data has been refetched
            if (release.MusicBrainzReleaseId == null && !release.ArtistHasBeenManuallyCorrected)
            {
                if (release.MusicBrainzReleaseId == null) //get initial artist call
                    artist = await _artistDataService.GetArtist(artist.DiscogsArtistId, true);

                await SaveReleasesFromMusicBrainzArtistCall(artist, release);
            }
            //using the artist id need to get releases and figure out which one is the right release
            //potential here for the release not being right.
            //The release may not exist on that call, this is using Levenshtein algorithm which is not right every time.
            if (!release.HasAllApiData || release.ArtistHasBeenManuallyCorrected)
            {
                var mostLikelyRelease = await GetMusicBrainzReleaseIdFromDiscogsReleaseInformation(release.Title, release.DiscogsArtistId ?? 0);
            
                if (mostLikelyRelease != null)
                {
                    release.IsAReleaseGroupGroupId = mostLikelyRelease.IsAReleaseGroupGroupId;
                    release.MusicBrainzReleaseId = mostLikelyRelease.MusicBrainzReleaseId;
                    await _db.UpdateAsync(release);
                }
            }
            release = await GetReleaseFromDbByDiscogsReleaseId(discogsReleaseId.Value);

            //releases having the image corrected should fall here
            if ((release.MusicBrainzReleaseId != null && !release.HasAllApiData) || release.ArtistHasBeenManuallyCorrected)
            {
                //now make the musicbrainz release call with the release id and get all the track lengths (if not a releasegroup) and original year
                var savedMusicBrainzTrackLengths = await MakeMusicBrainzReleaseCallAndSaveTracks(release, release.MusicBrainzReleaseId, release.IsAReleaseGroupGroupId);

            }
            var coverImages = await _db.GetAllEntitiesAsListAsync<MusicBrainzReleaseToCoverImage>();
            var coverImage = coverImages.Where(x => x.MusicBrainzReleaseId == release.MusicBrainzReleaseId).Select(x => x.MusicBrainzCoverImage).FirstOrDefault();
            if (release?.MusicBrainzReleaseId != null && coverImage == null)//various artist albums will not get a release id
            {
                coverImage = await GetCoverInfoAndReturnByteArrayImage(release.MusicBrainzReleaseId, release.IsAReleaseGroupGroupId);
                if (coverImage == null) { coverImage = Array.Empty<byte>(); }
            }
            release.HasAllApiData = true;//all api data retrieved for release
            release.ArtistHasBeenManuallyCorrected = false;//change back as all data is done until next correction
            release.ReleaseHasBeenManuallyCorrected = false;//change back as all data is done until next correction
            await _db.UpdateAsync(release);

            return (release, coverImage);
        }

        public async Task<byte[]?> GetImageForRelease(string musicBrainzReleaseId)
        {
            var releaseToCoverImages = await _db.GetAllEntitiesAsListAsync<MusicBrainzReleaseToCoverImage>();
            var list = releaseToCoverImages.ToList();
            return releaseToCoverImages.Where(x => x.MusicBrainzReleaseId == musicBrainzReleaseId).Select(x => x.MusicBrainzCoverImage).FirstOrDefault();
        }

        public async Task<(Release?, byte[]?)> GetRandomRelease()//not doing any checks for cover art/musicbrainz data - todo fix this
        {
            var releases = await _db.GetAllEntitiesAsListAsync<Release>();
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

        public async Task<List<Release>> GetNewestReleases(int howManyToReturn)
        {
            var returnedReleases = new List<Release>();

            var releases = await _db.GetAllEntitiesAsListAsync<Release>();

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
        private async Task<bool> MakeMusicBrainzReleaseCallAndSaveTracks(Release release, string? musicBrainzReleaseId, bool isAReleaseGroupUrl)
        {
            if (!isAReleaseGroupUrl)
            {
                //save tracks and release year
                var releaseData = await _musicBrainzApiService.GetReleaseFromMusicBrainzApiUsingMusicBrainsReleaseId(musicBrainzReleaseId);
                release.OriginalReleaseYear = releaseData.Date;
                await _db.UpdateAsync(release);

                var tracks = await _db.GetAllEntitiesAsListAsync<Track>();
                var tracksForThisRelease = tracks.Where(x => x.DiscogsReleaseId == release.DiscogsReleaseId).ToList();
                var tracksFromReleaseData = releaseData.Media.SelectMany(x => x.Tracks).ToList();
                if (!tracksFromReleaseData.Any()) { return true; }
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
            return true;
        }
        private async Task<bool> SaveTracksAndAdditionalInformationFromDiscogsReleaseResponse(DiscogsReleaseResponse releaseResponse)
        {
            try
            {
                var releaseTable = await _db.GetTable<Release>();
                var existingRelease = await releaseTable.Where(x => x.DiscogsReleaseId == releaseResponse.id).FirstOrDefaultAsync();
                var tracksTable = await _db.GetTable<Track>();
                var existingTracks = await tracksTable.Where(x => x.DiscogsReleaseId == releaseResponse.id).ToListAsync();
                if (existingRelease == null || existingRelease.DiscogsArtistId == null || existingRelease.DiscogsReleaseId == null)
                    //at this stage, dont want to store the release info if not in db already
                    throw new Exception($"Unhandled exception: Release {releaseResponse.id} not in database not able to store info.");

                await UpdateAdditionalReleaseProperties(releaseResponse, existingRelease);//todo: this could be moved to release data service

                await SaveTracksFromDiscogsReleaseResponse(releaseResponse, existingRelease, existingTracks);

                //save genres (styles) from release
                var success = await _discogsGenresAndTagsDataService.SaveStylesFromDiscogsRelease(releaseResponse, existingRelease.DiscogsReleaseId.Value, existingRelease.DiscogsArtistId.Value);

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at SaveArtistsFromCollectionResponse:{ex.Message} ");
                throw;
            }
        }
        private async Task SaveTracksFromDiscogsReleaseResponse(DiscogsReleaseResponse releaseResponse, Release existingRelease, List<Track> existingTracks)
        {
            //save the tracks
            if (existingTracks != null && !existingTracks.Any() && releaseResponse.tracklist != null)
            {
                foreach (var track in releaseResponse.tracklist)
                {
                    await _db.SaveItemAsync(new Track
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
        private async Task UpdateAdditionalReleaseProperties(DiscogsReleaseResponse releaseResponse, Release existingRelease)
        {
            //update existing release entity with additional properties
            existingRelease.ReleaseCountry = releaseResponse.country;
            existingRelease.ReleaseNotes = releaseResponse.notes;
            existingRelease.DiscogsReleaseUrl = releaseResponse.uri;
            await _db.UpdateAsync(existingRelease);
        }
        private async Task<Release?> GetReleaseFromDbByDiscogsReleaseId(int discogsReleaseId)
        {
            var releases = await _db.GetAllEntitiesAsListAsync<Release>();
            var release = releases.FirstOrDefault(x => x.DiscogsReleaseId == discogsReleaseId);
            return release;
        }
        private async Task<byte[]> GetCoverInfoAndReturnByteArrayImage(string savedReleaseId, bool isReleaseGroupId)
        {
            var coverApiResponse = await _coverArchiveApiService.GetCoverResponseByMusicBrainzReleaseId(savedReleaseId, isReleaseGroupId);
            if (coverApiResponse != null)
            {
                var releases = await _db.GetAllEntitiesAsListAsync<Release>();
                var releaseFromDb = releases.Where(x => x.MusicBrainzReleaseId == savedReleaseId).FirstOrDefault();

                var thumbnailUrls = coverApiResponse.Images.Select(x => x.Thumbnails).ToList();
                var coverUrl = thumbnailUrls.Select(x => x._500 ?? x.Small).FirstOrDefault();//choosing to save 500 or small one, can go bigger or larger 

                releaseFromDb.MusicBrainzCoverUrl = coverUrl;
                await _db.UpdateAsync(releaseFromDb);

                var coverByteArray = await _coverArchiveApiService.GetCoverByteArray(coverUrl);

                var releaseToCoverImageTable = await _db.GetTable<MusicBrainzReleaseToCoverImage>();

                var releaseToCoverImage = new MusicBrainzReleaseToCoverImage
                {
                    MusicBrainzReleaseId = savedReleaseId,
                    MusicBrainzCoverImage = coverByteArray
                };
                await _db.SaveItemAsync(releaseToCoverImage);

                return coverByteArray;
            }
            return null;
        }
        private async Task SaveReleasesFromMusicBrainzArtistCall(Artist artist, Release release)
        {
            try
            {
                if (artist.MusicBrainzArtistId == null)
                {
                    return;
                }
                var artistCallResponse = await _musicBrainzApiService.GetArtistFromMusicBrainzApiUsingArtistId(artist.MusicBrainzArtistId);

                var releasesByArtist = artistCallResponse.Releases?.ToList();
                var releaseGroupsByArtist = artistCallResponse.ReleaseGroups?.ToList();
                var existingJoins = await _db.GetAllEntitiesAsListAsync<MusicBrainzArtistToMusicBrainzRelease>();
                var existingReleasesForThisArtistList = existingJoins.Where(x => x.MusicBrainzArtistId == artist.MusicBrainzArtistId).Select(x => x.MusicBrainzReleaseId).ToList();

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
                            MusicBrainzArtistId = artist.MusicBrainzArtistId,
                            DiscogsArtistId = artist.DiscogsArtistId ?? 0,
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
                            MusicBrainzArtistId = artist.MusicBrainzArtistId,
                            DiscogsArtistId = artist.DiscogsArtistId ?? 0,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get data from API");
                throw;
            }
        }
        private async Task<MusicBrainzArtistToMusicBrainzRelease> GetMusicBrainzReleaseIdFromDiscogsReleaseInformation(string? discogsTitle, int discogsArtistId)
        {
        //using Fastenshtein.Levenshtein.Distance algorithm 
        https://github.com/DanHarltey/Fastenshtein
            //to get the most similar musicbrainz title based on the discogs title
            var releaseJoiningTable = await _db.GetAllEntitiesAsListAsync<MusicBrainzArtistToMusicBrainzRelease>();

            var releaseJoiningListByDiscogsArtist = releaseJoiningTable.Where(x => x.DiscogsArtistId == discogsArtistId).ToList();
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
            var savedCoverImage = await _db.GetTable<MusicBrainzReleaseToCoverImage>();
            var coverList = await savedCoverImage.ToListAsync();

            var imagesToRemove = coverList.Where(x => x.MusicBrainzReleaseId == allReleasesKnownByArtistId.Item1).ToList();

            foreach (var image in imagesToRemove)
            {
                await _db.DeleteAsync(image);
            }

            return allReleasesKnownByArtistId.Item2;
        }

        private async Task<(string, List<PossibleReleasesFromArtist>)> GetAllStoredMusicBrainzReleasesForArtistByDiscogsReleaseId(int? discogsReleaseId)
        {
            var releases = await _db.GetTable<Release>();
            var releasesList = await releases.ToListAsync();

            var incorrectReleases = releasesList.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();//could be multiples

            var discogsArtistId = incorrectReleases.FirstOrDefault().DiscogsArtistId;
            var badMusicBrainzReleaseId = incorrectReleases.FirstOrDefault().MusicBrainzReleaseId;

            var artists = await _db.GetTable<Artist>();
            var artistList = await artists.ToListAsync();
            var musicBrainzArtistId = artistList.Where(x => x.DiscogsArtistId == discogsArtistId).FirstOrDefault().MusicBrainzArtistId;

            var releaseJoiningTable = await _db.GetAllEntitiesAsListAsync<MusicBrainzArtistToMusicBrainzRelease>();
            var releaseJoiningListByMusicBrainzArtist = releaseJoiningTable.Where(x => x.MusicBrainzArtistId == musicBrainzArtistId).ToList();

            var allReleasesKnownByArtistId = releaseJoiningListByMusicBrainzArtist.Select(x => new PossibleReleasesFromArtist
            {
                Date = x.ReleaseYear,
                MusicBrainzReleaseId = x.MusicBrainzReleaseId,
                Status = x.Status,
                Title = x.MusicBrainzReleaseName
            }).ToList();
            return (badMusicBrainzReleaseId, allReleasesKnownByArtistId);
        }

        public async Task<bool> UpdateReleaseToBeNewMusicBrainzReleaseId(int? discogsReleaseId, string musicBrainzReleaseId)
        {
            var releaseTable = await GetAllReleasesAsList();
            var releasesToChange = releaseTable.Where(x => x.DiscogsReleaseId == discogsReleaseId).ToList();
            var musicBrainzReleaseData = await _db.GetTable<MusicBrainzArtistToMusicBrainzRelease>();
            var list = await musicBrainzReleaseData.ToListAsync();
            var newRelease = list.Where(x => x.MusicBrainzReleaseId == musicBrainzReleaseId).FirstOrDefault();
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

    }

    public class PossibleReleasesFromArtist
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string MusicBrainzReleaseId { get; set; }
    }
}
