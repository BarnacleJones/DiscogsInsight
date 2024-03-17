using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.ViewModels.Collection;
using DiscogsInsight.ViewModels.EntityViewModels;
using DiscogsInsight.ViewModels.Results;

namespace DiscogsInsight.View.Services.Collection
{
    public class CollectionViewService
    {
        private readonly CollectionDataService _collectionDataService;

        public CollectionViewService(CollectionDataService collectionDataService)
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
                    Genres = x.Genres,
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
