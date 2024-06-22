using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.Service.Models.Collection;
using DiscogsInsight.Service.Models.EntityViewModels;
using DiscogsInsight.Service.Models.Results;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.Service.Collection
{
    public class CollectionViewService
    {
        private readonly ICollectionDataService _collectionDataService;
        private readonly ILogger<CollectionViewService> _logger;

        public CollectionViewService(ICollectionDataService collectionDataService, ILogger<CollectionViewService> logger)
        {
            _collectionDataService = collectionDataService;
            _logger = logger;
        }

        private void LogError(Exception ex)
        {
            if (ex != null)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
            }
        }

        public async Task<ViewResult<DiscogsCollectionViewModel>> GetCollection()
        {
            try
            {
                var releases = await _collectionDataService.GetReleases();
                var releaseList = releases.ToList();

                var artistIdsList = await _collectionDataService.GetArtistsIdsAndNames();

                var viewModel = releaseList.Select(x => new ReleaseViewModel
                {
                    Artist = artistIdsList.Where(y => y.DiscogsArtistId == x.DiscogsArtistId).Select(x => x.Name).FirstOrDefault() ?? "No Name",
                    DiscogsArtistId = x.DiscogsArtistId,
                    DiscogsReleaseId = x.DiscogsReleaseId,
                    Year = x.Year.ToString(),
                    Title = x.Title,
                    DateAdded = x.DateAdded
                }).ToList();

                return new ViewResult<DiscogsCollectionViewModel>
                {
                    Data = new DiscogsCollectionViewModel { Releases = viewModel },
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new ViewResult<DiscogsCollectionViewModel>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }

        public async Task<ViewResult<DiscogsCollectionViewModel>> GetUntaggedCollection()
        {
            try
            {
                var releases = await _collectionDataService.GetReleases();

                var releaseList = releases.Where(x => !x.HasAllApiData).ToList();

                var artistIdsList = await _collectionDataService.GetArtistsIdsAndNames();

                var viewModel = releaseList.Select(x => new ReleaseViewModel
                {
                    Artist = artistIdsList.Where(y => y.DiscogsArtistId == x.DiscogsArtistId).Select(x => x.Name).FirstOrDefault() ?? "No Name",
                    DiscogsArtistId = x.DiscogsArtistId,
                    DiscogsReleaseId = x.DiscogsReleaseId,
                    Year = x.Year.ToString(),
                    Title = x.Title,
                    DateAdded = x.DateAdded
                }).ToList();

                return new ViewResult<DiscogsCollectionViewModel>
                {
                    Data = new DiscogsCollectionViewModel { Releases = viewModel },
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new ViewResult<DiscogsCollectionViewModel>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }
    }
}
