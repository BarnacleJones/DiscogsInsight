using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface IInsightsDataService
    {
        //Make all calls from the insights view services into here
        //Then can just do straight queries to db rther than loading tables

        //for now i just keep the entities but thats obvs what is in progress WIP
        Task<List<Track>> GetTrackInsightData();
        Task<List<Release>> GetAllReleasesDataModelsAsList();
    }
}
