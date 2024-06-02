using DiscogsInsight.DataAccess.Contract;

namespace DiscogsInsight.DataAccess.Services
{
    public class SettingsDataService : ISettingsDataService
    {
        private readonly ICollectionDataService _collectionDataService;
        private readonly IPreferencesService _preferencesService;

        public SettingsDataService(ICollectionDataService collectionDataService, IPreferencesService preferencesService)
        {
            _collectionDataService = collectionDataService;
            _preferencesService = preferencesService;
        }

        public async Task<bool> UpdateCollection()
        {
            return await _collectionDataService.CollectionSavedOrUpdatedFromDiscogs();
        }

        public string GetDiscogsUsername()
        {
            var username = _preferencesService.Get(PreferencesConstant.DiscogsUsername, "");
            return username;
        }

        public async Task<bool> UpdateDiscogsUsername(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                _preferencesService.Remove(PreferencesConstant.DiscogsUsername);
            }
            else
            {
                _preferencesService.Set(PreferencesConstant.DiscogsUsername, userName);
            }
            await _collectionDataService.PurgeEntireCollection();
            return true;
        }

        public async Task<bool> PurgeEntireDb()
        {
            await _collectionDataService.PurgeEntireDatabase();
            return true;
        }

        public Task<bool> UpdateLastFmSettings(string lastFmUsername, string lastFmPassword, string lastFmApiKey)
        {
            if (string.IsNullOrEmpty(lastFmUsername))
            {
                _preferencesService.Remove(PreferencesConstant.LastFmUserName);
            }
            else
            {
                _preferencesService.Set(PreferencesConstant.LastFmUserName, lastFmUsername);
            }

            if (string.IsNullOrEmpty(lastFmPassword))
            {
                _preferencesService.Remove(PreferencesConstant.LastFmPassword);
            }
            else
            {
                _preferencesService.Set(PreferencesConstant.LastFmPassword, lastFmPassword);
            }

            if (string.IsNullOrEmpty(lastFmApiKey))
            {
                _preferencesService.Remove(PreferencesConstant.LastFmApiKey);
            }
            else
            {
                _preferencesService.Set(PreferencesConstant.LastFmApiKey, lastFmApiKey);
            }

            return Task.FromResult(true);
        }

        public string GetLastFmApiKey()
        {
            var key = _preferencesService.Get(PreferencesConstant.LastFmApiKey, "");
            return key;
        }

        public string GetLastFmUsername()
        {
            var username = _preferencesService.Get(PreferencesConstant.LastFmUserName, "");
            return username;
        }

        public string GetLastFmPassword()
        {
            var pass = _preferencesService.Get(PreferencesConstant.LastFmPassword, "");
            return pass;
        }
    }
}
