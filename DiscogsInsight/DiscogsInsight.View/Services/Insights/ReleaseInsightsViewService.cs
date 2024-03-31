using DiscogsInsight.DataAccess.Entities;
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
                //get data
                var releases = await _releaseDataService.GetAllReleasesAsList();

                var releasesOverTimeLineChartSeriesData = GenerateDataForReleasesOverTimeGraph(releases);

                var data = new ReleaseInsightsStatsModel
                {
                    ReleasesOverTimeLineChartSeriesData = releasesOverTimeLineChartSeriesData
                };

                return new ViewResult<ReleaseInsightsStatsModel>
                {
                    Data = data,
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

        private static List<(string, double)> GenerateDataForReleasesOverTimeGraph(List<Release> releases)
        {
            var releasesOverTimeLineChartSeriesData = new List<(string, double)>();

            var groupedByYearReleases = releases.GroupBy(x => x.DateAdded.Value.Year).ToList().OrderBy(x => x.Key);

            
            foreach (var year in groupedByYearReleases)
            {
                var yearGroupedByMonth = year.GroupBy(x => x.DateAdded.Value.Month).ToList();
                bool startOfYear = true;
                foreach (var monthGroup in yearGroupedByMonth)
                {
                    var label = "";
                    if (startOfYear)
                    {
                        label = year.Key.ToString();
                        startOfYear = false;
                    }
                    releasesOverTimeLineChartSeriesData.Add((label, monthGroup.Count()));
                }
            }

            return releasesOverTimeLineChartSeriesData;
        }
    }
}
