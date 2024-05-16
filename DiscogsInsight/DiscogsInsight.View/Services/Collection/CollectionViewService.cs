using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.ViewModels.Collection;
using DiscogsInsight.ViewModels.EntityViewModels;
using DiscogsInsight.ViewModels.Results;

namespace DiscogsInsight.Service.Services.Collection
{
    public class CollectionViewService
    {
        private readonly ICollectionDataService _collectionDataService;

        public CollectionViewService(ICollectionDataService collectionDataService)
        {
            _collectionDataService = collectionDataService;
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
