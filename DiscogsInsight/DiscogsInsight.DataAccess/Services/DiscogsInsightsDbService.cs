//TODO remove this, was used to interface between the db class, but i dont think thats right. should db calss just be directly injected in each data service

//using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
//using DiscogsInsight.DataAccess.Entities;
//using DiscogsInsight.DataAccess.Interfaces;
//using Microsoft.Extensions.Logging;

//namespace DiscogsInsight.DataAccess.Services
//{
//    public class DiscogsInsightsDbService
//    {
//        private readonly DiscogsInsightDb _db;
//        private readonly ILogger<DiscogsInsightsDbService> _logger;
//        public DiscogsInsightsDbService(DiscogsInsightDb db, ILogger<DiscogsInsightsDbService> logger)
//        {
//            _db = db;
//            _logger = logger;
//        }

//        public async Task<List<T>> GetAllEntitiesAsync<T>() where T : IDatabaseEntity, new()
//        {
//            var entities = await _db.GetTable<T>();
//            return await entities.ToListAsync();
//        }

//        public async Task PurgeEntireDb()
//        {
//            await _db.Purge();
//        }

//        //dont want api methods here. they will be dealt with in their respective data services if nothing is returned from database using this service

//        //get entire collection and save (first time seed)
//        //this could be called in any method of specific data services


//        //public async Task<List<Release>> GetCollectionFromDiscogsAndSaveAndReturn() old from api service
//        //want to make this function be for a collection have discogs return a discogs response model, convert it here to entity handle data logic etc
       

//    }
//}
