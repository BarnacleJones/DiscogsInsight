using DiscogsInsight.DataModels;
using DiscogsInsight.ViewModels;

namespace DiscogsInsight.Services
{
    public class ReleaseService
    {
        private readonly CollectionDataService _db;
        private readonly DiscogsApiService _discogsApiService;

        public ReleaseService(CollectionDataService db, DiscogsApiService discogsApiService)
        {
            _db = db;
            _discogsApiService = discogsApiService;
        }



        public async Task<ReleaseViewModel> GetRelease(int? discogsReleaseId)
        {
            if (discogsReleaseId == null)
                throw new Exception($"Missing release info");

            var releaseTrackList = new List<TrackViewModel>();
            var releases = await _db.GetAllEntitiesAsync<Release>();
            var release = releases.FirstOrDefault(x => x.DiscogsReleaseId == discogsReleaseId);

            var artists = await _db.GetAllEntitiesAsync<Artist>();
            var artist = artists.FirstOrDefault(x => x.DiscogsArtistId == release.DiscogsArtistId);

            var tracks = await _db.GetAllEntitiesAsync<Track>();
            var releaseTracks = tracks.Where(x => x.DiscogsReleaseId == discogsReleaseId);

            if (release == null || !releaseTracks.Any())
            {
                var result = await _discogsApiService.GetReleaseFromDiscogsAndSave(discogsReleaseId.Value);

                var newRelease = await _db.GetAllEntitiesAsync<Release>();
                release = newRelease.FirstOrDefault(x => x.DiscogsReleaseId == discogsReleaseId);

                tracks = await _db.GetAllEntitiesAsync<Track>();
            }

            releaseTrackList = tracks.Where(x => x.DiscogsReleaseId == discogsReleaseId).Select(x => new TrackViewModel 
            {
                Title = x.Title,
                Duration = x.Duration,
                Position = x.Position
            })
            .ToList();
            return new ReleaseViewModel
            {
                Artist = artist.Name,
                Title = release.Title,
                DateAdded = release.DateAdded,
                Genres = release.Genres,
                Year = release.Year,
                Tracks = releaseTrackList,
                ReleaseCountry = release.ReleaseCountry
            };           
            
        }

        public async Task<ReleaseViewModel> GetRandomRelease()
        {

            var releases = await _db.GetAllEntitiesAsync<Release>();
            if (releases.Count < 1)
            {
                return new ReleaseViewModel
                {
                    Artist = "Nothing In collection"
                };
            }

            var randomRelease = releases.OrderBy(r => Guid.NewGuid()).FirstOrDefault();//new GUID as key, will be random

            if (randomRelease is null)
            {
                throw new Exception($"Error getting random release.");
            }

            var tracks = await _db.GetAllEntitiesAsync<Track>();
            var releaseTracks = tracks.Where(x => x.DiscogsReleaseId == randomRelease.DiscogsReleaseId);

            if (!releaseTracks.Any())
            {
                var result = await _discogsApiService.GetReleaseFromDiscogsAndSave(randomRelease.DiscogsReleaseId.Value);

                var newRelease = await _db.GetAllEntitiesAsync<Release>();
                randomRelease = newRelease.FirstOrDefault(x => x.DiscogsReleaseId == randomRelease.DiscogsReleaseId);

                tracks = await _db.GetAllEntitiesAsync<Track>();
            }


            var artists = await _db.GetAllEntitiesAsync<Artist>();
            var releaseArtistName = artists.Where(x => x.DiscogsArtistId == randomRelease.DiscogsArtistId).Select(x => x.Name).FirstOrDefault();

            var releaseTrackList = tracks.Where(x => x.DiscogsReleaseId == randomRelease.DiscogsReleaseId).Select(x => new TrackViewModel
            {
                Title = x.Title,
                Duration = x.Duration,
                Position = x.Position
            })
            .ToList();

            var viewModel = new ReleaseViewModel
            {
                Artist = releaseArtistName ?? "Missing Artist",
                Year = randomRelease.Year,
                Title = randomRelease.Title,
                Genres = randomRelease.Genres,
                Tracks = releaseTrackList,
                DateAdded = randomRelease.DateAdded
            };

            return viewModel;
        }

        public async Task<List<ReleaseViewModel>> GetNewestFiveReleases()
        {
            var returnedReleases = new List<ReleaseViewModel>();

            var releases = await _db.GetAllEntitiesAsync<Release>();
            if (releases.Count < 1)
            {
                return
                [
                    new ReleaseViewModel {Artist = "Nothing In collection" }
                ];
            }
            var releaseIsFiveOrMore = releases.Count() >= 5;
            var lastFiveReleases = releases.OrderByDescending(r => r.DateAdded).Take(releaseIsFiveOrMore ? 5 : 1);

            foreach (var item in lastFiveReleases)
            {
                var thisItem = item;
                var tracks = await _db.GetAllEntitiesAsync<Track>();
                var releaseTracks = tracks.Where(x => x.DiscogsReleaseId == thisItem.DiscogsReleaseId);

                if (!releaseTracks.Any())
                {
                    var result = await _discogsApiService.GetReleaseFromDiscogsAndSave(thisItem.DiscogsReleaseId.Value) ;

                    var newRelease = await _db.GetAllEntitiesAsync<Release>();
                    thisItem = newRelease.FirstOrDefault(x => x.DiscogsReleaseId == thisItem.DiscogsReleaseId);

                    tracks = await _db.GetAllEntitiesAsync<Track>();
                }

                var artists = await _db.GetAllEntitiesAsync<Artist>();
                var releaseArtistName = artists.Where(x => x.DiscogsArtistId == thisItem.DiscogsArtistId).Select(x => x.Name).FirstOrDefault();

                var releaseTrackList = tracks.Where(x => x.DiscogsReleaseId == thisItem.DiscogsReleaseId).Select(x => new TrackViewModel
                {
                    Title = x.Title,
                    Duration = x.Duration,
                    Position = x.Position
                })
                .ToList();

                returnedReleases.Add(new ReleaseViewModel
                {
                    Artist = releaseArtistName ?? "Missing Artist",
                    Year = item.Year,
                    Title = item.Title,
                    Genres = item.Genres,
                    Tracks = releaseTrackList,
                    DateAdded = item.DateAdded
                });

            }

            return returnedReleases;
        }
    }
}
