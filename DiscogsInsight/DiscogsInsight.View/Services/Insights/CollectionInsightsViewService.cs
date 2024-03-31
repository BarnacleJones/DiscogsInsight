using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.ViewModels.Insights;
using DiscogsInsight.ViewModels.Results;

namespace DiscogsInsight.View.Services.Insights
{
    public class CollectionInsightsViewService
    {
        private readonly CollectionDataService _collectionDataService;

        public CollectionInsightsViewService(CollectionDataService collectionDataService)
        {
            _collectionDataService = collectionDataService;
        }


        public async Task<ViewResult<CollectionInsightsStatsModel>> GetCollectionStatistics()
        {
            try
            {
                return new ViewResult<CollectionInsightsStatsModel>
                {
                    Data = null,
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<CollectionInsightsStatsModel>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }
    }
}
