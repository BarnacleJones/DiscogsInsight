using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.ViewModels.Collection;
using DiscogsInsight.ViewModels.EntityViewModels;

namespace DiscogsInsight.View.Services.Collection
{
    public class CollectionViewService
    {
        private readonly CollectionDataService _collectionDataService;

        public CollectionViewService(CollectionDataService collectionDataService)
        {
            _collectionDataService = collectionDataService;
        }

        public async Task<DiscogsCollectionViewModel> GetCollection()
        {
            var releases = await _collectionDataService.GetReleases();
            var releaseList = releases.ToList();
                       
            var artistIdsList = await _collectionDataService.GetArtistsIdsAndNames();

            var viewModel = releaseList.Select(x => new ReleaseViewModel
            {
                Artist = artistIdsList.Where(y => y.DiscogsArtistId == x.DiscogsArtistId).Select(x => x.Name).FirstOrDefault() ?? "No Name",
                DiscogsArtistId = x.DiscogsArtistId,
                DiscogsReleaseId = x.DiscogsReleaseId,
                Year = x.Year,
                Title = x.Title,
                Genres = x.Genres,
                DateAdded = x.DateAdded
            }).ToList();

            return new DiscogsCollectionViewModel { Releases = viewModel };
        }

        public async Task<int> GetCollectionSize()
        {
            var releases = await _collectionDataService.GetReleases();
            return releases.ToList().Count();
        }
    }
}
