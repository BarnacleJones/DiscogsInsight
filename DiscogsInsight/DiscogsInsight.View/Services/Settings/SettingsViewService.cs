using DiscogsInsight.DataAccess.Services;

namespace DiscogsInsight.View.Services.Settings
{
    public class SettingsViewService
    {
        private readonly SettingsDataService _settingsDataService;

        public SettingsViewService(SettingsDataService settingsDataService)
        {
            _settingsDataService = settingsDataService;
        }

        public async Task<bool> UpdateCollection()
        {
            return await _settingsDataService.UpdateCollection();
        }

        public string GetDiscogsUsername()
        {
            return _settingsDataService.GetDiscogsUsername();
        }

        public async Task<bool> UpdateDiscogsUsername(string userName)
        {
            return await _settingsDataService.UpdateDiscogsUsername(userName);
        }

    }
}

