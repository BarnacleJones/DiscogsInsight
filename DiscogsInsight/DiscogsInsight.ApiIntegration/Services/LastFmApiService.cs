using DiscogsInsight.ApiIntegration.Contract;
using DiscogsInsight.DataAccess.Contract;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

namespace DiscogsInsight.ApiIntegration.Services
{


    public class LastFmApiService : ILastFmApiService
    {
        private readonly LastfmClient _lastFmClient;
        private readonly IPreferencesService _preferencesService;
        private readonly HttpClient _httpClient;
        private string _lastFmUserName;        
        private string _lastFmPassword;        
        private string _lastFmApiKey;
        private string _lastFmApiSecret;

        public LastFmApiService(IPreferencesService preferencesService, IHttpClientFactory httpClientFactory)
        {
            _preferencesService = preferencesService;
            _httpClient = httpClientFactory.CreateClient("LastFmApiClient");
            _lastFmApiKey = _preferencesService.Get(PreferencesConstant.LastFmApiKey, "");
            _lastFmApiSecret = _preferencesService.Get(PreferencesConstant.LastFmApiSecret, "");
            _lastFmUserName = _preferencesService.Get(PreferencesConstant.LastFmUserName, "");
            _lastFmPassword = _preferencesService.Get(PreferencesConstant.LastFmPassword, "");

            _lastFmClient = new LastfmClient(_lastFmApiKey, _lastFmApiSecret, _httpClient);
        }

        public async Task<LastAlbum> GetAlbumInformation(string artistName, string albumName)
        {
            await EnsureAuthenticatedAsync();

            var response = await _lastFmClient.Album.GetInfoAsync(artistName, albumName, true);

            if (!response.Success)
            {
                throw new Exception($"Unsuccessful API connection: {response.Status}");
            }

            return response.Content ?? throw new Exception("Album information is null");

        }

        public async Task<LastResponse> ScrobbleRelease(Scrobble scrobble)
        {
            await EnsureAuthenticatedAsync();
            var scrobbleResponse = await _lastFmClient.Scrobbler.ScrobbleAsync(scrobble);

            return scrobbleResponse ?? throw new Exception("Error scrobbling release");
        }

        public async Task<LastResponse> ScrobbleReleases(List<Scrobble> scrobbles)
        {
            await EnsureAuthenticatedAsync();
            var scrobbleResponse = await _lastFmClient.Scrobbler.ScrobbleAsync(scrobbles);

            return scrobbleResponse ?? throw new Exception("Error scrobbling release");
        }

        private async Task EnsureAuthenticatedAsync()
        {
            if (!_lastFmClient.Auth.Authenticated)
            {
                var authenticationResponse = await _lastFmClient.Auth.GetSessionTokenAsync(_lastFmUserName, _lastFmPassword);

                if (!authenticationResponse.Success)
                {
                    throw new Exception($"Authentication failed: {authenticationResponse.Status}");
                }
            }
        }
    }
}

