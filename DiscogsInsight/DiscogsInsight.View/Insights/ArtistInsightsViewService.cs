using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.Service.Models.Insights;
using DiscogsInsight.Service.Models.Results;

namespace DiscogsInsight.Service.Insights
{
    public class ArtistInsightsViewService
    {
        private readonly IInsightsDataService _insightsDataService;

        public ArtistInsightsViewService(IInsightsDataService insightsDataService)
        {
            _insightsDataService = insightsDataService;
        }


        public async Task<ViewResult<ArtistInsightsStatsModel>> GetArtistStatistics()
        {
            try
            {
                return new ViewResult<ArtistInsightsStatsModel>
                {
                    Data = null,
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<ArtistInsightsStatsModel>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }

    }
}
