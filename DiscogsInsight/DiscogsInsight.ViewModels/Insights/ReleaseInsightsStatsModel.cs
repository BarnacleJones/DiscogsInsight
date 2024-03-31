﻿namespace DiscogsInsight.ViewModels.Insights
{
    public class ReleaseInsightsStatsModel
    {
        public List<(string XAxisLabelMonthAndYear, double releasesThatMonth )> ReleasesOverTimeLineChartSeriesData { get; set; }

        public string AverageReleasePressingYear { get; set; }
        public string EarliestReleaseYear { get; set; }
        
    }

}
