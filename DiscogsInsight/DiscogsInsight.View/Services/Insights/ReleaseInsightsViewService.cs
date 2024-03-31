using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.ViewModels.Insights;
using DiscogsInsight.ViewModels.Results;

namespace DiscogsInsight.View.Services.Insights
{
    public class ReleaseInsightsViewService
    {
        private readonly ReleaseDataService _releaseDataService;

        public ReleaseInsightsViewService(ReleaseDataService releaseDataService)
        {
            _releaseDataService = releaseDataService;
        }

        public async Task<ViewResult<ReleaseInsightsStatsModel>> GetReleaseStatistics()
        {
            try
            {
                return new ViewResult<ReleaseInsightsStatsModel>
                {
                    Data = null,
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<ReleaseInsightsStatsModel>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }

    }
}
