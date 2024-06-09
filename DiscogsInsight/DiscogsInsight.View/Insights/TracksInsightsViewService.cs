using DiscogsInsight.Database.Entities;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.Service.Models.Insights;
using DiscogsInsight.Service.Models.Results;
using DiscogsInsight.DataAccess.Services;

namespace DiscogsInsight.Service.Insights
{
    public class TracksInsightsViewService
    {
        private readonly InsightsDataService _insightsDataService;

        public TracksInsightsViewService(InsightsDataService trackDataService)
        {
            _insightsDataService = trackDataService;
        }

        public async Task<ViewResult<TracksInsightsStatsModel>> GetTracksStatistics()
        {
            try
            {
                var tracks = await _insightsDataService.GetTrackInsightData();

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
