namespace DiscogsInsight.Services
{
    public class SettingsService
    {
        private readonly DiscogsApiService _discogsApiService;

        public SettingsService(DiscogsApiService discogsApiService)
        { 
            _discogsApiService = discogsApiService;
        }

        public async Task<bool> UpdateCollection()
        {
            return await _discogsApiService.UpdateCollection();
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
