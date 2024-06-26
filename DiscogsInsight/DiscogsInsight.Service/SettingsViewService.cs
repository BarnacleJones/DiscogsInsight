﻿using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.Service.Models.Results;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.Service
{
    public class SettingsViewService
    {
        private readonly ISettingsDataService _settingsDataService;
        private readonly ILogger<SettingsViewService> _logger;

        public SettingsViewService(ISettingsDataService settingsDataService, ILogger<SettingsViewService> logger)
        {
            _settingsDataService = settingsDataService;
            _logger = logger;
        }

        private void LogError(Exception ex)
        {
            if (ex != null)
            {
                _logger.LogError(ex.StackTrace);
                _logger.LogError(ex.Message);
            }

        }
        public async Task<ViewResult<bool>> UpdateCollection()
        {
            try
            {
                var success = await _settingsDataService.UpdateCollection();

                if (success)
                {
                    return new ViewResult<bool> { Success = true, ErrorMessage = "" };
                }

                return new ViewResult<bool> { Success = false, ErrorMessage = "Error Updating Collection. " };

            }
            catch (Exception ex)
            {
                LogError(ex);
                return new ViewResult<bool>() { Success = false, ErrorMessage = ex.Message };
            }
        }
        public string GetDiscogsUsername()
        {
            return _settingsDataService.GetDiscogsUsername();
        }
        public bool HasSavedDiscogsUsername()
        {
            return _settingsDataService.ContainsKey(PreferencesConstant.DiscogsUsername);
        }
        public string GetLastFmApiKey()
        {
            return _settingsDataService.GetLastFmApiKey();
        }
        public string GetLastFmUsername()
        {
            return _settingsDataService.GetLastFmUsername();
        }
        public string GetLastFmPassword()
        {
            return _settingsDataService.GetLastFmPassword();
        }
        public async Task<ViewResult<bool>> UpdateDiscogsUsername(string userName)
        {
            try
            {
                var success = await _settingsDataService.UpdateDiscogsUsername(userName);

                if (success)
                {
                    return new ViewResult<bool> { Success = true, ErrorMessage = "" };
                }

                return new ViewResult<bool> { Success = false, ErrorMessage = "Error Updating Discogs Username. " };
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new ViewResult<bool>() { Success = false, ErrorMessage = ex.Message };
            }
        }
        public async Task<ViewResult<bool>> SaveUpdateLastFmSettings(string lastFmUsername, string lastFmPassword, string lastFmApiKey)
        {
            try
            {
                var success = await _settingsDataService.UpdateLastFmSettings(lastFmUsername, lastFmPassword, lastFmApiKey);

                if (success)
                {
                    return new ViewResult<bool> { Success = true, ErrorMessage = "" };
                }

                return new ViewResult<bool> { Success = false, ErrorMessage = "Error Updating Last Fm Details. " };
            }
            catch (Exception ex)
            {
                LogError(ex);
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
                    return new ViewResult<bool> { Success = true, ErrorMessage = "" };
                }

                return new ViewResult<bool> { Success = false, ErrorMessage = "Error PurgingDatabase. " };
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new ViewResult<bool>() { Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}

