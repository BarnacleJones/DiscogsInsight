namespace DiscogsInsight.DataAccess.Contract
{
    public interface ISettingsDataService
    {
        Task<bool> UpdateCollection();
        string GetDiscogsUsername();
        Task<bool> UpdateDiscogsUsername(string userName);
        Task<bool> PurgeEntireDb();
        Task<bool> UpdateLastFmSettings(string lastFmUsername, string lastFmPassword, string lastFmApiKey);
        string GetLastFmApiKey();
        string GetLastFmUsername();
        string GetLastFmPassword();
    }
}
