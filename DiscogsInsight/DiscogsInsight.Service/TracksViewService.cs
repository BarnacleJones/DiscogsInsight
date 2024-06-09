using DiscogsInsight.DataAccess.Contract;

/* Unmerged change from project 'DiscogsInsight.Service (net8.0-windows10.0.19041.0)'
Before:
using DiscogsInsight.Service.Models.Collection;
After:
using DiscogsInsight.Service;
using DiscogsInsight.Service;
using DiscogsInsight.Service.Models.Collection;
*/
using DiscogsInsight.Service.Models.Collection;
using DiscogsInsight.Service.Models.Results;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.Service
{
    public class TracksViewService
    {
        private readonly ITracksDataService _tracksDataService;
        private readonly IReleaseDataService _releaseDataService;
        private readonly IArtistDataService _artistDataService;
        private readonly ILogger<TracksViewService> _logger;
        public TracksViewService(ITracksDataService tracksDataService, ILogger<TracksViewService> logger, IReleaseDataService releaseDataService, IArtistDataService artistDataService)
        {
            _tracksDataService = tracksDataService;
            _logger = logger;
            _releaseDataService = releaseDataService;
            _artistDataService = artistDataService;
        }


        public async Task<bool> SetRatingOnTrack(int? rating, int discogsReleaseId, string title)
        {
            var success = await _tracksDataService.SetRatingOnTrack(rating, discogsReleaseId, title);
            return success;
        }

        //Decided in the refactor to get rid of random track. Leaving this here 08/06/2024 in case I change my mind
        //Needs refactoring lol
        //public async Task<ViewResult<TracksItemViewModel>> GetRandomTrack()
        //{
        //    try
        //    {
        //        var tracks = await _tracksDataService.GetAllTracks();

        //        var random = new Random();
        //        var randomTrack = tracks.OrderBy(x => random.Next()).FirstOrDefault();

        //        if (randomTrack != null)
        //        {
        //            var releases = await _releaseDataService.GetAllReleasesAsList();
        //            var artists = await _artistDataService.GetAllArtistsFromDatabase();
        //            var releaseTitle = releases.Where(x => x.DiscogsReleaseId == randomTrack.DiscogsReleaseId).Select(x => x.Title).FirstOrDefault();
        //            var releaseArtist = artists.Where(x => x.DiscogsArtistId == randomTrack.DiscogsArtistId).Select(x => x.Name).FirstOrDefault();

        //            var data = new TracksItemViewModel
        //            {
        //                DiscogsArtistId = randomTrack.DiscogsArtistId ?? 0,
        //                DiscogsReleaseId = randomTrack.DiscogsReleaseId ?? 0,
        //                Duration = randomTrack.MusicBrainzTrackLength == null
        //                            ? randomTrack.Duration
        //                            : TimeSpan.FromMilliseconds(randomTrack.MusicBrainzTrackLength.Value).ToString(@"mm\:ss"),
        //                Title = randomTrack.Title,
        //                Position = randomTrack.Position,
        //                Rating = randomTrack.Rating ?? 0,
        //                Artist = releaseArtist,
        //                Release = releaseTitle
        //            };

        //            return new ViewResult<TracksItemViewModel>
        //            {
        //                Data = data,
        //                ErrorMessage = "",
        //                Success = true
        //            };
        //        }
        //        return new ViewResult<TracksItemViewModel>
        //        {
        //            Data = null,
        //            ErrorMessage = "Random track was null",
        //            Success = false
        //        };

        //    }
        //    catch (Exception ex)
        //    {
        //        return new ViewResult<TracksItemViewModel>
        //        {
        //            Data = null,
        //            ErrorMessage = ex.Message,
        //            Success = false
        //        };
        //    }
        //}

        public async Task<ViewResult<TracksGridViewModel>> GetTracksForTracksGrid()
        {
            try
            {
                var tracks = await _tracksDataService.GetAllTracksForGrid();

                var tracksAsGridItems = tracks.Select(x => new TracksItemViewModel
                {
                    DiscogsArtistId = x.DiscogsArtistId ?? 0,
                    DiscogsReleaseId = x.DiscogsReleaseId ?? 0,
                    Duration = x.MusicBrainzTrackLength == null
                                    ? x.Duration
                                    : TimeSpan.FromMilliseconds(x.MusicBrainzTrackLength.Value).ToString(@"mm\:ss"),
                    Title = x.Title,
                    Position = x.Position,
                    Rating = x.Rating ?? 0
                }).ToList();

                var releases = await _releaseDataService.GetAllReleasesAsList();
                var artists = await _artistDataService.GetAllArtistsFromDatabase();

                foreach (var tracksItems in tracksAsGridItems)
                {
                    var releaseTitle = releases.Where(x => x.DiscogsReleaseId == tracksItems.DiscogsReleaseId).Select(x => x.Title).FirstOrDefault();
                    tracksItems.Release = releaseTitle;

                    var releaseArtist = artists.Where(x => x.DiscogsArtistId == tracksItems.DiscogsArtistId).Select(x => x.Name).FirstOrDefault();
                    tracksItems.Artist = releaseArtist;
                }

                return new ViewResult<TracksGridViewModel>
                {
                    Data = new TracksGridViewModel { Tracks = tracksAsGridItems },
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<TracksGridViewModel>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }
    }
}
