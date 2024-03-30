using DiscogsInsight.DataAccess;
using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.ViewModels.Collection;
using DiscogsInsight.ViewModels.Results;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.View.Services.Tracks
{
    public class TracksViewService
    {
        private readonly TracksDataService _tracksDataService;
        private readonly ReleaseDataService _releaseDataService;
        private readonly ArtistDataService _artistDataService;
        private readonly ILogger<TracksViewService> _logger;
        public TracksViewService(TracksDataService tracksDataService, ILogger<TracksViewService> logger, ReleaseDataService releaseDataService, ArtistDataService artistDataService)
        {
            _tracksDataService = tracksDataService;
            _logger = logger;
            _releaseDataService = releaseDataService;
            _artistDataService = artistDataService;
        }


        public async Task<ViewResult<TracksGridViewModel>> GetTracksForTracksGrid()
        {
            try
            {
                var tracks = await _tracksDataService.GetAllTracks();

                var tracksAsGridItems = tracks.Select(x => new TracksGridItemViewModel
                {
                    DiscogsArtistId = x.DiscogsArtistId ?? 0,
                    DiscogsReleaseId = x.DiscogsReleaseId ?? 0,
                    Duration = x.Duration,
                    Title = x.Title,
                    Position = x.Position,
                    Rating = x.Rating
                }).ToList();

                var releases = await _releaseDataService.GetAllReleasesAsList();
                var artists = await _artistDataService.GetArtists();
                       
                foreach ( var tracksItems in tracksAsGridItems) 
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
