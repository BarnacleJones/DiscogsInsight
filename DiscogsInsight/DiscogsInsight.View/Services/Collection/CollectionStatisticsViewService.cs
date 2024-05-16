using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.ViewModels;
using DiscogsInsight.ViewModels.Results;

namespace DiscogsInsight.Service.Services.Collection
{
    public class CollectionStatisticsViewService
    {
        private readonly ICollectionDataService _collectionDataService;
        private readonly ITracksDataService _tracksDataService;

        public CollectionStatisticsViewService(ICollectionDataService collectionDataService, ITracksDataService tracksDataService)
        {
            _collectionDataService = collectionDataService;
            _tracksDataService = tracksDataService;
        }

        public async Task<ViewResult<CollectionStatsViewModel>> GetHomePageStatistics()
        {
            try
            {
                var data = await GetCollectionStatsViewModel();

                return new ViewResult<CollectionStatsViewModel>
                {
                    Data = data,
                    ErrorMessage = "",
                    Success = true
                };

            }
            catch (Exception ex)
            {
                return new ViewResult<CollectionStatsViewModel>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }

        private async Task<CollectionStatsViewModel> GetCollectionStatsViewModel()
        {
            var releases = await _collectionDataService.GetReleases();
            var releasesCount = releases.Count();

            var tracks = await _tracksDataService.GetAllTracks();
            var tracksCount = tracks.Count();
            var tracksReleasesList = tracks.Select(x => x.DiscogsReleaseId).ToList();

            var releasesWithoutAllApiData = releases.Where(x => !x.HasAllApiData).ToList().Count;

            var month = DateTime.Now.Month;
            var tracksNewThisMonth = releases.Where(x => x.DateAdded.Value.Month == month).Count();

            //get last 6 months albums 
            var lastSixMonths = new List<double>();
            for (int i = 5; i >= 0; i--)
            {
                var thisParticularMonth = DateTime.Today.AddMonths(-i).Month;
                var thisParticularYear = DateTime.Today.AddMonths(-i).Year;
                var tracksNewThisParticularMonth = releases.Where(x => x.DateAdded.Value.Month == thisParticularMonth && x.DateAdded.Value.Year == thisParticularYear).Count();
                lastSixMonths.Add(tracksNewThisParticularMonth);
            }


            return new CollectionStatsViewModel
            {
                AlbumsInCollection = releasesCount,
                AlbumsInCollectionWithoutTracksInfo = releasesWithoutAllApiData,
                TracksInCollection = tracksCount,
                AlbumsNewToCollectionThisMonth = tracksNewThisMonth,
                AlbumsNewLastSixMonths = lastSixMonths.ToArray()
            };
        }
    }
}
