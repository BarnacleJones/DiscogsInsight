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
                return new ViewResult<TracksInsightsStatsModel>
                {
                    Data = null,
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
    }
}
