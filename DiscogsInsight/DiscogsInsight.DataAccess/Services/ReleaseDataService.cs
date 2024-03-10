using DiscogsInsight.ApiIntegration.Services;
using DiscogsInsight.DataAccess.Entities;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.DataAccess.Services
{
    public class ReleaseDataService
    {
        private readonly DiscogsInsightDb _db;
        private readonly MusicBrainzApiService _musicBrainzApiService;
        private readonly CoverArtArchiveApiService _coverArchiveApiService;
        private readonly CollectionDataService _collectionDataService;
        private readonly ArtistDataService _artistDataService;
        private readonly ILogger<ReleaseDataService> _logger;

        public ReleaseDataService(DiscogsInsightDb db, MusicBrainzApiService musicBrainzApiService,ArtistDataService artistDataService , CollectionDataService collectionDataService, CoverArtArchiveApiService coverArchiveApiService, ILogger<ReleaseDataService> logger)
        {
            _db = db;
            _musicBrainzApiService = musicBrainzApiService;
            _collectionDataService = collectionDataService;
            _artistDataService = artistDataService; 
            _coverArchiveApiService = coverArchiveApiService;
            _logger = logger;
        }

        public async Task<Release?> GetReleaseFromDbByDiscogsReleaseId(int discogsReleaseId)
        {
            var releases = await _db.GetAllEntitiesAsync<Release>();
            var release = releases.FirstOrDefault(x => x.DiscogsReleaseId == discogsReleaseId);
            return release;
        }

        public async Task<Release?> GetRelease(int? discogsReleaseId)
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
            var artists = await _db.GetAllEntitiesAsync<Artist>();
            var artist = artists.FirstOrDefault(x => x.DiscogsArtistId == release.DiscogsArtistId);

            if (artist == null) throw new Exception("No artist ????");
            if (release == null) throw new Exception("No release ????");

            if (release.MusicBrainzReleaseId == null || release.MusicBrainzCoverUrl == null)
            {  
                if(release.MusicBrainzReleaseId == null) //get initial artist call
                    artist = await _artistDataService.GetArtist(release.DiscogsArtistId, true);
                
                //using the artist id need to get releases and figure out which one is the right release
                var savedRelease = await SaveReleasesFromMusicBrainzArtistCallAndReturnTheMusicBrainzReleaseInfo(artist, release);
                
                release.IsAReleaseGroupGroupId = savedRelease.IsAReleaseGroupGroupId;                
                release.MusicBrainzReleaseId = savedRelease.MusicBrainzReleaseId;
                await _db.UpdateAsync(release);

                var coverImage = GetCoverInfoAndReturnByteArrayImage(savedRelease.MusicBrainzReleaseId, savedRelease.IsAReleaseGroupGroupId);

                await _db.SaveItemAsync(release);

                release = await GetReleaseFromDbByDiscogsReleaseId(discogsReleaseId.Value);
            }
            
            return release;
        }

        private async Task<byte[]> GetCoverInfoAndReturnByteArrayImage(string savedReleaseId, bool isReleaseGroupId)
        {
            var coverApiResponse = await _coverArchiveApiService.GetCoverResponseByMusicBrainzReleaseId(savedReleaseId, isReleaseGroupId);

            var releases = await _db.GetAllEntitiesAsync<Release>();
            var releaseFromDb = releases.Where(x => x.MusicBrainzReleaseId == savedReleaseId).FirstOrDefault();

            var thumbnailUrls = coverApiResponse.Images.Select(x => x.Thumbnails).ToList();
            var coverUrl = thumbnailUrls.Select(x => x._500 ?? x.Small).FirstOrDefault();//choosing to save 500 or small one, can go bigger or larger 

            releaseFromDb.MusicBrainzCoverUrl = coverUrl;
            await _db.UpdateAsync(releaseFromDb);

            var coverByteArray = await _coverArchiveApiService.GetCoverByteArray(coverUrl);

            releaseFromDb.MusicBrainzCoverImage = coverByteArray;

            await _db.UpdateAsync(releaseFromDb);

            return coverByteArray;
        }

        public async Task<MusicBrainzArtistToMusicBrainzRelease> SaveReleasesFromMusicBrainzArtistCallAndReturnTheMusicBrainzReleaseInfo(Artist artist, Release release)
        {
            try
            {
                var artistCallResponse = await _musicBrainzApiService.GetArtistFromMusicBrainzApiUsingArtistId(artist.MusicBrainzArtistId);
                
                var releasesByArtist = artistCallResponse.Releases?.ToList();
                var releaseGroupsByArtist = artistCallResponse.ReleaseGroups?.ToList();
                var existingJoins = await _db.GetAllEntitiesAsync<MusicBrainzArtistToMusicBrainzRelease>();
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

                var mostLikelyRelease = await GetMusicBrainzReleaseIdFromDiscogsReleaseInformation(release.Title, release.DiscogsArtistId ?? 0);
                return mostLikelyRelease;

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
            var releaseJoiningTable = await _db.GetAllEntitiesAsync<MusicBrainzArtistToMusicBrainzRelease>();

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

        public async Task<Release> GetRandomRelease()//not doing any checks for cover art/musicbrainz data - todo fix this
        {
            var releases = await _db.GetAllEntitiesAsync<Release>();
            if (releases.Count < 1)
            {
                releases = await _collectionDataService.GetReleases();
            }

            var randomRelease = releases.Select(x => x.DiscogsReleaseId).OrderBy(r => Guid.NewGuid()).FirstOrDefault();//new GUID as key, will be random

            if (randomRelease is null)
            {
                throw new Exception($"Error getting random release.");
            }

            var release = await GetRelease(randomRelease);//get additional release info from apis if they dont exist

            return release;          
        }

        public async Task<List<Release>> GetNewestReleases(int howManyToReturn)
        {
            var returnedReleases = new List<Release>();

            var releases = await _db.GetAllEntitiesAsync<Release>();

            if (releases.Count < 1)//seed the collection if no releases
            {
                releases = await _collectionDataService.GetReleases();
            }
            var moreReleasesThanHowManyToReturn = releases.Count() >= howManyToReturn;

            return releases.OrderByDescending(r => r.DateAdded)
                           .Take(moreReleasesThanHowManyToReturn ? howManyToReturn : 1)
                           .ToList();
        }
               
    }
}
