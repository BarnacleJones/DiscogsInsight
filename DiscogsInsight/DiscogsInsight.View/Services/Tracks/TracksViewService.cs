using DiscogsInsight.DataAccess;
using DiscogsInsight.DataAccess.Services;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.View.Services.Tracks
{
    public class TracksViewService
    {
        private readonly DiscogsInsightDb _db;
        private readonly TracksDataService _tracksDataService;
        private readonly ILogger<TracksViewService> _logger;
        public TracksViewService(TracksDataService tracksDataService, DiscogsInsightDb db, ILogger<TracksViewService> logger)
        {
            _tracksDataService = tracksDataService;
            _logger = logger;
            _db = db;
        }
     
        //will be used for tracks view pages
    }    
}
