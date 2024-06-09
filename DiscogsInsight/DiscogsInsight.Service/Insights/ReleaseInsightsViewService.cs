using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.Database.Entities;
using DiscogsInsight.Service.Models.Insights;
using DiscogsInsight.Service.Models.Results;

namespace DiscogsInsight.Service.Insights
{
    public class ReleaseInsightsViewService
    {
        private readonly IInsightsDataService _insightsDataService;

        public ReleaseInsightsViewService(IInsightsDataService insightsDataService)
        {
            _insightsDataService = insightsDataService;
        }

        public async Task<ViewResult<ReleaseInsightsStatsModel>> GetReleaseStatistics()
        {
            try
            {
                //get data
                //var releases = await _releaseDataService.GetAllReleasesAsList();
                var releases = await _insightsDataService.GetAllReleasesDataModelsAsList();

                var earliestRelease = releases.Where(x => !string.IsNullOrWhiteSpace(x.OriginalReleaseYear))
                                              .OrderBy(x => x.OriginalReleaseYear)
                                              .FirstOrDefault();

                var earliestReleaseText = $"{earliestRelease.OriginalReleaseYear} - {earliestRelease.Title}";

                var releasePressingYears = releases.Where(x => x.Year.HasValue && x.Year.Value > 0)
                                                   .OrderBy(x => x.Year)
                                                   .Select(x => x.Year)
                                                   .ToList();

                var averagePressingYear = releasePressingYears.Average() ?? 0;

                var averagePressingYearText = Math.Round(averagePressingYear, 0, MidpointRounding.AwayFromZero).ToString();


                var releasesOverTimeLineChartSeriesData = GenerateDataForReleasesOverTimeGraph(releases);

                var releasesByYearAndLabelsData = GenerateDataForReleasesByPressingCountry(releases);

                var data = new ReleaseInsightsStatsModel
                {
                    ReleasesOverTimeLineChartSeriesData = releasesOverTimeLineChartSeriesData,
                    EarliestReleaseYear = earliestReleaseText ?? "",
                    AverageReleasePressingYear = averagePressingYearText,
                    ReleasesPressedCountryLabels = releasesByYearAndLabelsData.Item1,
                    ReleasesPressedCountryValues = releasesByYearAndLabelsData.Item2
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

        private (string[], double[]) GenerateDataForReleasesByPressingCountry(List<Release>? releases)
        {
            var releasesGroupedByCountryOfRelease = releases
                .GroupBy(x => x.ReleaseCountry)
                .Where(x => x.Key != null)
                .ToList();
            var dataList = new List<(string, double)>();

            foreach (var country in releasesGroupedByCountryOfRelease)
            {
                dataList.Add((country.Key ?? "Unknown", country.Count()));//should no longer get unknown
            }

            var labels = new string[dataList.Count()];
            var countryCounts = new double[dataList.Count()];
            for (int i = 0; i < dataList.Count; i++)
            {
                labels[i] = dataList[i].Item1;
                countryCounts[i] = dataList[i].Item2;
            }

            return (labels, countryCounts);
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
