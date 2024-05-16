using DiscogsInsight.ApiIntegration.Contract.Services;
using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Logging;
namespace DiscogsInsight.ApiIntegration.Services
{


    public class LastFmApiService : ILastFmApiService
    {
        private readonly LastfmClient _lastFmClient;
        private readonly ILogger<LastFmApiService> _logger;
        private string _lastFmUserName;        
        private string _lastFmPassword;        
        private string _lastFmApiKey;

        //-----------------------------------------------------------------------------
        //Hard coded LastFm API Endpoints. If they ever change this should be the only place they are referenced!

        //private const string whateverisneeded = "blah";

        //-----------------------------------------------------------------------------

        public LastFmApiService(LastfmClient lastFmClient, ILogger<LastFmApiService> logger)
        {
            _lastFmClient = lastFmClient;
            _logger = logger;
            _lastFmUserName = Preferences.Default.Get("lastFmUsername", "Unknown");
            _lastFmPassword = Preferences.Default.Get("lastFmPassword", "Unknown");
            _lastFmApiKey = Preferences.Default.Get("lastFmApiKey", "Unknown");
        }

    }
}

