using SQLitePCL;

namespace DiscogsInsight.Services
{
    public class SettingsService
    {
        private readonly DiscogsApiService _discogsApiService;
        private readonly CollectionDataService _collectionDataService;

        public SettingsService(DiscogsApiService discogsApiService, CollectionDataService collectionDataService)
        { 
            _discogsApiService = discogsApiService;
            _collectionDataService = collectionDataService;
        }

        public async Task<bool> UpdateCollection()
        {
            var result =  await _discogsApiService.GetCollectionFromDiscogsAndSaveAndReturn();
            return result.Count > 0;
        }

        public string GetDiscogsUsername()
        {
            var username = Preferences.Default.Get("discogsUsername", "");
            return username;
        }

        public async Task<bool> UpdateDiscogsUsername(string userName)
        {
            if (userName == string.Empty)
            {
                Preferences.Default.Remove("discogsUsername");
            }
            else
            {
                Preferences.Default.Set("discogsUsername", userName);
            }
            await _collectionDataService.PurgeEntireDb();
            return true;
        }

    }
}
