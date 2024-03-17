using DiscogsInsight.ApiIntegration.MusicBrainzResponseModels;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DiscogsInsight.ApiIntegration.Services
{
    public class MusicBrainzApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MusicBrainzApiService> _logger;

        //-----------------------------------------------------------------------------
        //Hard coded MusicBrainz API Endpoints. If they ever change this should be the only place they are referenced!

        //MusicBrainzApi documentation: https://musicbrainz.org/doc/MusicBrainz_API


        private const string InitialArtistRequest = "/ws/2/artist/?query=artist:";

        //know about release groups - thats what you want at this stage, its the main release info https://musicbrainz.org/doc/Release_Group

        private const string ReleaseGroupUrl = "/ws/2/release-group/940a8468-73dd-4c0c-94a8-823b1b13c736";
        private const string ReleaseGroupIncludeUrl = "?inc=artists+releases";

        private const string ReleaseUrl = "/ws/2/release/59211ea4-ffd2-4ad9-9a4e-941d3148024a";
        private const string ReleaseIncludeUrl = "?inc=artist-credits+labels+discids+recordings+tags";

        private const string ArtistUrl = "/ws/2/artist/";
        private const string ArtistIncludeUrl = "?inc=aliases+releases+release-groups";

        //-----------------------------------------------------------------------------


        public MusicBrainzApiService(IHttpClientFactory httpClientFactory, ILogger<MusicBrainzApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("MusicBrainzApiClient");
            _logger = logger;            
        }

        public async Task<MusicBrainzInitialArtist> GetInitialArtistFromMusicBrainzApi(string artistName)
        {
            try
            {
                var responseData = new MusicBrainzInitialArtist();

                var fullArtistRequestUrl = InitialArtistRequest + $"'{artistName}'";
                
                var response = await _httpClient.GetAsync(fullArtistRequestUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                responseData = JsonSerializer.Deserialize<MusicBrainzInitialArtist>(json);

                return responseData == null 
                    ? throw new Exception("Error getting musicbrainz artist data") 
                    : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get data from API");
                throw;
            }
        }        
        public async Task<MusicBrainzArtist> GetArtistFromMusicBrainzApiUsingArtistId(string musicBrainzArtistId)
        {
            try
            {
                var responseData = new MusicBrainzArtist();

                var fullArtistRequestUrl = ArtistUrl + $"{musicBrainzArtistId}" + ArtistIncludeUrl;
                
                var response = await _httpClient.GetAsync(fullArtistRequestUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                responseData = JsonSerializer.Deserialize<MusicBrainzArtist>(json);

                return responseData == null 
                    ? throw new Exception("Error getting musicbrainz artist data") 
                    : responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get data from API");
                throw;
            }
        }
    }
}
