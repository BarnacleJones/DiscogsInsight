using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using DiscogsInsight.DataAccess.Entities;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.DataAccess.Services
{
    public class DiscogsGenresAndTagsDataService
    {
        private readonly DiscogsInsightDb _db;
        private readonly ILogger<DiscogsGenresAndTagsDataService> _logger;
        private readonly TagsDataService _tagsDataService;

        public DiscogsGenresAndTagsDataService(DiscogsInsightDb db, 
            ILogger<DiscogsGenresAndTagsDataService> logger, 
            TagsDataService tagsDataService)
        {
            _db = db;
            _logger = logger;
            _tagsDataService = tagsDataService;
        }

        public async Task<bool> SaveTagsFromDiscogsRelease(DiscogsReleaseResponse releaseResponse, int discogsReleaseId, int discogsArtistId)
        {

            var discogsGenreTagsToDiscogsReleaseTable = await _db.GetTable<DiscogsGenreTagToDiscogsRelease>();
            var discogsGenreTags = await _db.GetTable<DiscogsGenreTags>();

            var existingGenreTags = await tracksTable.Where(x => x.DiscogsReleaseId == releaseResponse.id).ToListAsync();
        }
    }
}
