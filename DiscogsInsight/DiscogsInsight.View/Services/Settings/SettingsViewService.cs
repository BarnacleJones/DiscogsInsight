using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.ViewModels.Results;

namespace DiscogsInsight.View.Services.Settings
{
    public class SettingsViewService
    {
        private readonly SettingsDataService _settingsDataService;

        public SettingsViewService(SettingsDataService settingsDataService)
        {
            _settingsDataService = settingsDataService;
        }

        public async Task<ViewResult<bool>> UpdateCollection()
        {
            try
            {
             var success = await _settingsDataService.UpdateCollection();

                if (success)
                {
                    return new ViewResult<bool> { Success = true, ErrorMessage="" };
                }

                return new ViewResult<bool> { Success = false, ErrorMessage = "Error Updating Collection. " };

            }
            catch (Exception ex)
            {
                return new ViewResult<bool>() { Success = false, ErrorMessage = ex.Message };
            }
        }

        public string GetDiscogsUsername()
        {
            return _settingsDataService.GetDiscogsUsername();
        }

        public async Task<ViewResult<bool>> UpdateDiscogsUsername(string userName)
        {
            try
            {
                var success = await _settingsDataService.UpdateDiscogsUsername(userName);

                if (success)
                {
                    return new ViewResult<bool> { Success = true, ErrorMessage="" };
                }

                return new ViewResult<bool> { Success = false, ErrorMessage = "Error Updating Discogs Username. " };
            }
            catch (Exception ex)
            {
                return new ViewResult<bool>() { Success = false, ErrorMessage = ex.Message };
            }
        }
        public async Task<ViewResult<bool>> PurgeEntireDatabase()
        {
            try
            {
                var success = await _settingsDataService.PurgeEntireDb();

                if (success)
                {
                    return new ViewResult<bool> { Success = true, ErrorMessage="" };
                }

                return new ViewResult<bool> { Success = false, ErrorMessage = "Error PurgingDatabase. " };
            }
            catch (Exception ex)
            {
                return new ViewResult<bool>() { Success = false, ErrorMessage = ex.Message };
            }
        }

    }
}

