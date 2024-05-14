using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Entities;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.DataAccess.Services
{
    public class DiscogsGenresAndTagsDataService : IDiscogsGenresAndTagsDataService
    {
        private readonly IDiscogsInsightDb _db;
        private readonly ILogger<DiscogsGenresAndTagsDataService> _logger;
        private readonly ITagsDataService _tagsDataService;

        public DiscogsGenresAndTagsDataService(IDiscogsInsightDb db, 
            ILogger<DiscogsGenresAndTagsDataService> logger, 
            ITagsDataService tagsDataService)
        {
            _db = db;
            _logger = logger;
            _tagsDataService = tagsDataService;
        }

        public async Task<List<DiscogsGenreTags>> GetAllGenreTagsAsList()
        {
            return await _db.GetAllEntitiesAsListAsync<DiscogsGenreTags>();

        }
        public async Task<List<DiscogsGenreTagToDiscogsRelease>> GetDiscogsGenreTagToDiscogsReleaseAsList()
        {
            return await _db.GetAllEntitiesAsListAsync<DiscogsGenreTagToDiscogsRelease>();
        }

        public async Task<List<(string?, int)>> GetGenresForDiscogsRelease(int? discogsReleaseId)
        {
            if (discogsReleaseId == null) { return new List<(string?, int)>(); };

            var discogsGenreJoiningTableList = await GetDiscogsGenreTagToDiscogsReleaseAsList();
            
            var genreIdsForThisRelease = discogsGenreJoiningTableList.Where(x => x.DiscogsReleaseId == discogsReleaseId).Select(x => x.DiscogsGenreTagId).ToList();
            
            var genreTable = await GetAllGenreTagsAsList();

            return genreTable.Where(x => genreIdsForThisRelease.Contains(x.Id)).Select(x => (x.DiscogsTag, x.Id)).ToList();

        }

        public async Task<bool> SaveStylesFromDiscogsRelease(DiscogsReleaseResponse releaseResponse, int discogsReleaseId, int discogsArtistId)
        {
            try
            {
                var discogsGenreTagsList = await GetAllGenreTagsAsList();

                if (releaseResponse.styles == null || !releaseResponse.styles.Any() || discogsGenreTagsList == null)
                {
                    return false;
                }

                var releaseStylesFromReleaseResponse = releaseResponse.styles;
                var stylesNotInDatabaseAlready = releaseStylesFromReleaseResponse.Except(discogsGenreTagsList.Select(y => y.DiscogsTag)).ToList();

                if (stylesNotInDatabaseAlready.Any())
                {
                    foreach (var style  in stylesNotInDatabaseAlready)
                    {
                        await _db.InsertAsync(new DiscogsGenreTags { DiscogsTag = style});
                    }

                    discogsGenreTagsList = await GetAllGenreTagsAsList();
                }

                foreach (var style in releaseStylesFromReleaseResponse)
                {
                    var genreTagId = discogsGenreTagsList.Where(x => x.DiscogsTag == style).Select(x => x.Id).FirstOrDefault();

                    await _db.InsertAsync(new DiscogsGenreTagToDiscogsRelease 
                    {
                        DiscogsReleaseId = discogsReleaseId,
                        DiscogsArtistId = discogsArtistId,
                        DiscogsGenreTagId = genreTagId
                    });
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<bool> SaveGenresFromDiscogsRelease(ResponseRelease responseRelease, int? discogsReleaseId, int? discogsArtistId)
        {
            try
            {
                var discogsGenreTagsList = await GetAllGenreTagsAsList();
                                
                if (responseRelease == null) return true;//dont think this is possible

                var releaseGenresFromReleaseResponse = responseRelease.basic_information.genres;

                if (releaseGenresFromReleaseResponse == null) return true;//dont want to error, there may just be no genres associated

                var stylesNotInDatabaseAlready = releaseGenresFromReleaseResponse.Except(discogsGenreTagsList.Select(y => y.DiscogsTag)).ToList();

                if (stylesNotInDatabaseAlready.Any())
                {
                    foreach (var style in stylesNotInDatabaseAlready)
                    {
                        await _db.InsertAsync(new DiscogsGenreTags { DiscogsTag = style });
                    }

                    discogsGenreTagsList = await GetAllGenreTagsAsList();
                }

                foreach (var style in releaseGenresFromReleaseResponse)
                {
                    var genreTagId = discogsGenreTagsList.Where(x => x.DiscogsTag == style).Select(x => x.Id).FirstOrDefault();

                    await _db.InsertAsync(new DiscogsGenreTagToDiscogsRelease
                    {
                        DiscogsReleaseId = discogsReleaseId,
                        DiscogsArtistId = discogsArtistId,
                        DiscogsGenreTagId = genreTagId
                    });
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }

            
        }
    }
}
