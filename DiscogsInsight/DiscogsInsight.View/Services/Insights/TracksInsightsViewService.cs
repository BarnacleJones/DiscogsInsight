using DiscogsInsight.DataAccess.Entities;
using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.ViewModels.Insights;
using DiscogsInsight.ViewModels.Results;

namespace DiscogsInsight.View.Services.Insights
{
    public class TracksInsightsViewService
    {
        private readonly TracksDataService _trackDataService;

        public TracksInsightsViewService(TracksDataService trackDataService)
        {
            _trackDataService = trackDataService;
        }

        public async Task<ViewResult<TracksInsightsStatsModel>> GetTracksStatistics()
        {
            try
            {
                var tracks = await _trackDataService.GetAllTracks();

                var averageTrackLengthFormatted = GetAverageTrackLengthStringFormatted(tracks);
                var averageTracksPerReleaseText = GetAverageTracksPerReleaseStringFormatted(tracks);

                var data = new TracksInsightsStatsModel
                {
                    AverageTrackLength = averageTrackLengthFormatted,
                    AverageTracksPerRelease = averageTracksPerReleaseText
                };
                return new ViewResult<TracksInsightsStatsModel>
                {
                    Data = data,
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<TracksInsightsStatsModel>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }

        private static string GetAverageTracksPerReleaseStringFormatted(List<Track> tracks)
        {
            var releasesToTracks = tracks.GroupBy(x => x.DiscogsReleaseId).ToList();

            var tracksPerReleaseCount = new List<int>();
            foreach (var release in releasesToTracks)
            {
                tracksPerReleaseCount.Add(release.Count());
            }
            var averageTracksPerReleaseText = Math.Round(tracksPerReleaseCount.Average(), 0, MidpointRounding.AwayFromZero).ToString();
            return averageTracksPerReleaseText;
        }

        private static string GetAverageTrackLengthStringFormatted(List<Track> tracks)
        {
            var unformattedAverageTrackLength = tracks
                                                .Where(x => x.MusicBrainzTrackLength != null)
                                                .Average(x => x.MusicBrainzTrackLength);
            var averageTrackLengthFormatted = "";
            if (unformattedAverageTrackLength.HasValue)
            {
                averageTrackLengthFormatted = $"{TimeSpan.FromMilliseconds(unformattedAverageTrackLength.Value).ToString(@"mm\:ss")}";
            }

            return averageTrackLengthFormatted;
        }
    }
}
