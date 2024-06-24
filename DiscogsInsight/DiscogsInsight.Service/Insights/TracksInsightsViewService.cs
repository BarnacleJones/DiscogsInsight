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

        public async Task<ViewResult<TracksInsightsStatsViewModel>> GetTracksStatistics()
        {
            try
            {
                var tracks = await _insightsDataService.GetTrackInsightData();

                var averageTrackLengthString = TimeSpan.FromMilliseconds(tracks.AverageTrackLength).ToString(@"mm\:ss");
                var averageTacksPerRelease = Math.Round(tracks.AverageTracksPerRelease.Average(), 0, MidpointRounding.AwayFromZero).ToString();

                var data = new TracksInsightsStatsViewModel
                {
                    AverageTrackLength = tracks.AverageTrackLength,
                    AverageTracksPerRelease = tracks.AverageTracksPerRelease
                };
                return new ViewResult<TracksInsightsStatsViewModel>
                {
                    Data = data,
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<TracksInsightsStatsViewModel>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }

    }
}
