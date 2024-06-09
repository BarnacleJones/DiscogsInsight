using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.Database.Contract;
using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.DataAccess.Services
{
    public class InsightsDataService : IInsightsDataService
    {
        private readonly ISQLiteAsyncConnection _db;

        public InsightsDataService(ISQLiteAsyncConnection db)
        {
            _db = db;
        }

        public Task<List<Release>> GetAllReleasesDataModelsAsList()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Track>> GetTrackInsightData()
        {
            //todo refactor this to return just the needed track data. use a query
            //return await _db.Table<Track>().ToListAsync();
        }
    }
}
