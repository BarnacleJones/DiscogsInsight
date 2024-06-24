using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface IInsightsDataService
    {
        Task<TracksInsightDataModel> GetTrackInsightData();
        Task<List<Release>> GetAllReleasesDataModelsAsList();
    }
}
