using DiscogsInsight.ApiIntegration.MusicBrainzResponseModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DiscogsInsight.ApiIntegration.Services
{
    public class MusicBrainzApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MusicBrainzApiService> _logger;
        private string _discogsUserName;

        //-----------------------------------------------------------------------------
        //Hard coded MusicBrainz API Endpoints. If they ever change this should be the only place they are referenced!

        //MusicBrainzApi documentation: https://musicbrainz.org/doc/MusicBrainz_API


        //private const string InitialArtistRequest = "/release-group/?query=artist:\"michael jackson\"";
        private const string InitialArtistRequest = "/ws/2/artist/?query=artist:";
        private const string InitialArtistIncludeUrl = "?inc=aliases";

        //know about release groups - thats what you want at this stage, its the main release info https://musicbrainz.org/doc/Release_Group

        private const string ReleaseGroupUrl = "/ws/2/release-group/940a8468-73dd-4c0c-94a8-823b1b13c736";
        private const string ReleaseGroupIncludeUrl = "?inc=artists+releases";

        private const string ReleaseUrl = "/ws/2/release/59211ea4-ffd2-4ad9-9a4e-941d3148024a";
        private const string ReleaseIncludeUrl = "?inc=artist-credits+labels+discids+recordings+tags";

        private const string ArtistUrl = "/ws/2/artist/b574bfea-2359-4e9d-93f6-71c3c9a2a4f0";
        private const string ArtistIncludeUrl = "?inc=aliases+releases";


        //Cover images, just add the release id - note NOT the release-group id

        private const string ImageUrl = "coverartarchive.org/release/";

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

                var fullArtistRequestUrl = InitialArtistRequest + $"'{artistName}'" + InitialArtistIncludeUrl;
                
                var response = await _httpClient.GetAsync(fullArtistRequestUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                responseData = JsonConvert.DeserializeObject<MusicBrainzInitialArtist>(json);

                if (responseData == null)
                        throw new Exception("Error getting musicbrainz artist data");

                return responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get data from API");
                throw;
            }
        }        
    }
}
