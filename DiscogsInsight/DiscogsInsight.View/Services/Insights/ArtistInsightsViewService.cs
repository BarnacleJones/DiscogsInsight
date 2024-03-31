using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.ViewModels.Insights;
using DiscogsInsight.ViewModels.Results;

namespace DiscogsInsight.View.Services.Insights
{
    public class ArtistInsightsViewService
    {
        private readonly ArtistDataService _artistDataService;

        public ArtistInsightsViewService(ArtistDataService artistDataService)
        {
            _artistDataService = artistDataService;
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
