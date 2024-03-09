using DiscogsInsight.ApiIntegration.Services;
using DiscogsInsight.DataAccess.Entities;

namespace DiscogsInsight.DataAccess.Services
{
    public class ReleaseDataService
    {
        private readonly DiscogsInsightDb _db;
        private readonly MusicBrainzApiService _musicBrainzApiService;
        private readonly CollectionDataService _collectionDataService;

        public ReleaseDataService(DiscogsInsightDb db, MusicBrainzApiService musicBrainzApiService, CollectionDataService collectionDataService)
        {
            _db = db;
            _musicBrainzApiService = musicBrainzApiService;
            _collectionDataService = collectionDataService;
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
            if (release.MusicBrainzReleaseId == null)
            {
                var artists = await _db.GetAllEntitiesAsync<Artist>();
                var artist = artists.FirstOrDefault(x => x.DiscogsArtistId == release.DiscogsArtistId);
                var apiReleaseModel = await _musicBrainzApiService.GetReleaseIdAndCoverArtUrlFromMusicBrainzApiByReleaseTitle(release.Title, release.DiscogsArtistId.Value, artist.MusicBrainzArtistId);

                release.MusicBrainzReleaseId = apiReleaseModel.ReleaseId;
                release.MusicBrainzCoverUrl = apiReleaseModel.CoverArtUrl;
                await _db.UpdateAsync(release);

                release = await GetReleaseFromDbByDiscogsReleaseId(discogsReleaseId.Value);
            }
            return release;
        }

        public async Task<Release> GetRandomRelease()
        {
            var releases = await _db.GetAllEntitiesAsync<Release>();
            if (releases.Count < 1)
            {
                releases = await _collectionDataService.GetReleases();
            }

            var randomRelease = releases.OrderBy(r => Guid.NewGuid()).FirstOrDefault();//new GUID as key, will be random

            if (randomRelease is null)
            {
                throw new Exception($"Error getting random release.");
            }

            return randomRelease;          
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
