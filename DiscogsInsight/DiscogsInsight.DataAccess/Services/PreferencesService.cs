using DiscogsInsight.DataAccess.Contract;

namespace DiscogsInsight.DataAccess.Services
{
    public class PreferencesService : IPreferencesService
    {
        public string Get(string key, string defaultValue)
        {
            return Preferences.Default.Get(key, defaultValue);
        }

        public void Set(string key, string value)
        {
            Preferences.Default.Set(key, value);
        }

        public void Remove(string key)
        {
            Preferences.Default.Remove(key);
        }
    }
}
