using DiscogsInsight.ApiIntegration.Contract;
using DiscogsInsight.DataAccess.Contract;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

namespace DiscogsInsight.ApiIntegration.Services
{


    public class LastFmApiService : ILastFmApiService
    {
        private readonly LastfmClient _lastFmClient;
        private readonly IPreferencesService _preferencesService;
        private string _lastFmUserName;        
        private string _lastFmPassword;        
        private string _lastFmApiKey;
        private string _lastFmApiSecret;

        //-----------------------------------------------------------------------------
        //Hard coded LastFm API Endpoints. If they ever change this should be the only place they are referenced!

        //private const string whateverisneeded = "blah";

        //-----------------------------------------------------------------------------

        public LastFmApiService(IPreferencesService preferencesService)
        {
            _preferencesService = preferencesService;

            _lastFmApiKey = _preferencesService.Get(PreferencesConstant.LastFmApiKey, "");
            _lastFmApiSecret = _preferencesService.Get(PreferencesConstant.LastFmApiSecret, "");
            _lastFmUserName = _preferencesService.Get(PreferencesConstant.LastFmUserName, "");
            _lastFmPassword = _preferencesService.Get(PreferencesConstant.LastFmPassword, "");

            HttpClient _httpClient = new HttpClient();
            _lastFmClient = new LastfmClient(_lastFmApiKey, _lastFmApiSecret, _httpClient);
            //authenticate
            //_ = _lastFmClient.Auth.GetSessionTokenAsync(_lastFmUserName, _lastFmPassword);
        }

        public async Task<LastAlbum> GetAlbumInformation(string artistName, string albumName)
        {
            var response = await _lastFmClient.Album.GetInfoAsync(artistName, albumName);
            if (response.Status != LastResponseStatus.Successful)
            {
                throw new Exception("Unsuccessful API connection");
            }

            LastAlbum albumInfo = response.Content;

            if (albumInfo != null)
            {
                return albumInfo;
            }
            throw new Exception("Album information is null");
        }

        public async Task<LastResponse> ScrobbleRelease(Scrobble scrobble)
        {
            if (_lastFmClient.Auth.Authenticated)
            {
                var scrobbleResponse = await _lastFmClient.Scrobbler.ScrobbleAsync(scrobble);

                if (scrobbleResponse != null)
                {
                    return scrobbleResponse;
                }
            }
            else
            {
                var authenticationResponse = await _lastFmClient.Auth.GetSessionTokenAsync(_lastFmUserName, _lastFmPassword);
                
                var scrobbleResponse = await _lastFmClient.Scrobbler.ScrobbleAsync(scrobble);

                if (scrobbleResponse != null)
                {
                    return scrobbleResponse;
                }
            }

            throw new Exception("Error scrobbling release"); 
        }

        public async Task<LastResponse> ScrobbleReleases(List<Scrobble> scrobbles)
        {
            if (_lastFmClient.Auth.Authenticated)
            {
                var scrobbleResponse = await _lastFmClient.Scrobbler.ScrobbleAsync(scrobbles);

                if (scrobbleResponse != null)
                {
                    return scrobbleResponse;
                }
            }
            else
            {
                var authenticationResponse = await _lastFmClient.Auth.GetSessionTokenAsync(_lastFmUserName, _lastFmPassword);

                var scrobbleResponse = await _lastFmClient.Scrobbler.ScrobbleAsync(scrobbles);

                if (scrobbleResponse != null)
                {
                    return scrobbleResponse;
                }
            }

            throw new Exception("Error scrobbling release");
        }

    }
}

