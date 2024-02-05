﻿namespace DiscogsInsight.Services
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
            var result =  await _discogsApiService.GetCollectionFromDiscogsAndSaveAndReturn();
            return result.Count > 0;
        }

        public string GetDiscogsUsername()
        {
            var username = Preferences.Default.Get("discogsUsername", "");
            return username;
        }

        public bool UpdateDiscogsUsername(string userName)
        {
            if (userName == string.Empty)
            {
                Preferences.Default.Remove("discogsUsername");
            }
            else
            {
                Preferences.Default.Set("discogsUsername", userName);
            }
            //await _db.Purge();
            return true;
        }

    }
}
