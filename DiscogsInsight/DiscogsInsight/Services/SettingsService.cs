namespace DiscogsInsight.Services
{
    public class SettingsService
    {
        private readonly DiscogsInsightDb _db;
        private readonly DiscogsApiService _discogsApiService;

        public SettingsService(DiscogsInsightDb db, DiscogsApiService discogsApiService)
        { 
            _db = db;
            _discogsApiService = discogsApiService;
        }

        public async Task<bool> UpdateCollection()
        {
            //List<Release> releaseList;
            //var releases = await _db.GetAllEntitiesAsync<Release>();
            //releaseList = releases.ToList();

            //if (!releaseList.Any())
            //{
            //    releaseList = await _discogsApiService.GetCollectionFromDiscogsAndSaveAndReturn();

            //}
            //var artists = await _db.GetAllEntitiesAsync<Artist>();
            //var artistIdsList = artists.Select(x => new { x.DiscogsArtistId, x.Name }).ToList();

            //var viewModel = releaseList.Select(x => new ReleaseViewModel
            //{
            //    Artist = artistIdsList.Where(y => y.DiscogsArtistId == x.ArtistId).Select(x => x.Name).FirstOrDefault() ?? "ERROR",//Todo: make this better
            //    Year = x.Year,
            //    Title = x.Title,
            //    ResourceUrl = x.ResourceUrl,
            //    MasterUrl = x.MasterUrl,
            //    Genres = x.Genres,
            //    DateAdded = x.DateAdded
            //}).ToList();
            return true;
            //return new DiscogsCollection { Releases = viewModel };
        }

        public string GetDiscogsUsername()
        {
            var username = Preferences.Default.Get("discogsUsername", "");
            return username;
        }

        public bool UpdateDiscogsUsername(string userName)
        {
            Preferences.Default.Set("discogsUsername", userName);
            return true;
        }

    }
}
