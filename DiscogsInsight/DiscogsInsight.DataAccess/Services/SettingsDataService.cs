using DiscogsInsight.DataAccess.Contract;

namespace DiscogsInsight.DataAccess.Services
{
    public class SettingsDataService
    {
        private readonly ICollectionDataService _collectionDataService;

        public SettingsDataService(ICollectionDataService collectionDataService)
        {
            _collectionDataService = collectionDataService;
        }

        public async Task<bool> UpdateCollection()
        {
            return await _collectionDataService.CollectionSavedOrUpdatedFromDiscogs();
        }

        public string GetDiscogsUsername()
        {
            var username = Preferences.Default.Get(Constants.DiscogsUsername, "");
            return username;
        }

        public async Task<bool> UpdateDiscogsUsername(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                Preferences.Default.Remove(Constants.DiscogsUsername);
            }
            else
            {
                Preferences.Default.Set(Constants.DiscogsUsername, userName);
            }
            await _collectionDataService.PurgeEntireCollection();
            return true;
        }
        
        public async Task<bool> PurgeEntireDb()
        {            
            await _collectionDataService.PurgeEntireDatabase();
            return true;
        }

        public static Task<bool> UpdateLastFmSettings(string lastFmUsername, string lastFmPassword, string lastFmApiKey)
        {
            if (string.IsNullOrEmpty(lastFmUsername))
            {
                Preferences.Default.Remove(Constants.LastFmUserName);
            }
            else
            {
                Preferences.Default.Set(Constants.LastFmUserName, lastFmUsername);
            }

            if (string.IsNullOrEmpty(lastFmPassword))
            {
                Preferences.Default.Remove(Constants.LastFmPassword);
            }
            else
            {
                Preferences.Default.Set(Constants.LastFmPassword, lastFmPassword);
            }

            if (string.IsNullOrEmpty(lastFmApiKey))
            {
                Preferences.Default.Remove(Constants.LastFmApiKey);
            }
            else
            {
                Preferences.Default.Set(Constants.LastFmApiKey, lastFmApiKey);
            }

            return Task.FromResult(true);
        }

        public string GetLastFmApiKey()
        {
            var key = Preferences.Default.Get(Constants.LastFmApiKey, "");
            return key;
        }

        public string GetLastFmUsername()
        {
            var username = Preferences.Default.Get(Constants.LastFmUserName, "");
            return username;
        }

        public string GetLastFmPassword()
        {
            var pass = Preferences.Default.Get(Constants.LastFmPassword, "");
            return pass;
        }
    }
}
