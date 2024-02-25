namespace DiscogsInsight.DataAccess.Services
{
    public class SettingsDataService
    {
        private readonly CollectionDataService _collectionDataService;

        public SettingsDataService(CollectionDataService collectionDataService)
        {
            _collectionDataService = collectionDataService;
        }

        public async Task<bool> UpdateCollection()
        {
            return await _collectionDataService.CollectionSavedOrUpdatedFromDiscogs();
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
            await _collectionDataService.PurgeEntireCollection();
            return true;
        }

    }
}
