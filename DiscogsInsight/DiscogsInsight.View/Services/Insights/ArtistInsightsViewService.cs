﻿using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.ViewModels.Insights;
using DiscogsInsight.ViewModels.Results;

namespace DiscogsInsight.Service.Services.Insights
{
    public class ArtistInsightsViewService
    {
        private readonly IArtistDataService _artistDataService;

        public ArtistInsightsViewService(IArtistDataService artistDataService)
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
