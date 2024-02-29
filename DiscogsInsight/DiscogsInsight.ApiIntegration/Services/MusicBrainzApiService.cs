using DiscogsInsight.ApiIntegration.MusicBrainzResponseModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;

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


        //private const string InitialArtistRequest = "http://musicbrainz.org/ws/2/release-group/?query=artist:\"michael jackson\"";
        private const string InitialArtistRequest = "http://musicbrainz.org/ws/2/artist/?query=artist:";
        private const string InitialArtistIncludeUrl = "?inc=aliases";

        //know about release groups - thats what you want at this stage, its the main release info https://musicbrainz.org/doc/Release_Group

        private const string ReleaseGroupUrl = "http://musicbrainz.org/ws/2/release-group/940a8468-73dd-4c0c-94a8-823b1b13c736";
        private const string ReleaseGroupIncludeUrl = "?inc=artists+releases";

        private const string ReleaseUrl = "http://musicbrainz.org/ws/2/release/59211ea4-ffd2-4ad9-9a4e-941d3148024a";
        private const string ReleaseIncludeUrl = "?inc=artist-credits+labels+discids+recordings+tags";

        private const string ArtistUrl = "http://musicbrainz.org/ws/2/artist/b574bfea-2359-4e9d-93f6-71c3c9a2a4f0";
        private const string ArtistIncludeUrl = "?inc=aliases+releases";


        //Cover images, just add the release id - note NOT the release-group id

        private const string ImageUrl = "coverartarchive.org/release/";

        //-----------------------------------------------------------------------------


        public MusicBrainzApiService(HttpClient httpClient, ILogger<MusicBrainzApiService> logger)
        {
            _discogsUserName = Preferences.Default.Get("discogsUsername", "Unknown");
            if (string.IsNullOrEmpty(_discogsUserName))
            {
                _logger.LogError("Empty username");
            }

            _httpClient = httpClient;
            _logger = logger;
            
        }

        public async Task<MusicBrainzInitialArtist> GetInitialArtistFromMusicBrainzApi(string artistName)
        {
            try
            {
                var responseData = new MusicBrainzInitialArtist();

                var fullArtistRequestUrl = InitialArtistRequest + $"'{artistName}'" + InitialArtistIncludeUrl;
                _httpClient.DefaultRequestHeaders.Add("User-Agent", $"DiscogsInsight_{_discogsUserName}");
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                _httpClient.BaseAddress = new Uri(fullArtistRequestUrl);

                var response = await _httpClient.GetAsync(fullArtistRequestUrl);
                //response.EnsureSuccessStatusCode();

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
        //public async Task<DiscogsReleaseResponse> GetReleaseFromDiscogs(int discogsReleaseId)
        //{
        //    try
        //    {
        //        var thisReleaseUrl = ReleaseUrl + discogsReleaseId;
        //        var response = await _httpClient.GetAsync(thisReleaseUrl);
        //        response.EnsureSuccessStatusCode();

        //        var json = await response.Content.ReadAsStringAsync();
        //        var responseData = JsonConvert.DeserializeObject<DiscogsReleaseResponse>(json);

        //        if (responseData == null)
        //        {
        //            throw new Exception("Error getting data");
        //        }

        //        return responseData;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Failed to get data from API: {ex.Message}");
        //    }
        //}
        //public async Task<DiscogsArtistResponse> GetArtistFromDiscogs(int discogsArtistId)
        //{
        //    try
        //    {
        //        var thisArtistUrl = ArtistUrl + discogsArtistId;
        //        var response = await _httpClient.GetAsync(thisArtistUrl);
        //        response.EnsureSuccessStatusCode();

        //        var json = await response.Content.ReadAsStringAsync();
        //        var responseData = JsonConvert.DeserializeObject<DiscogsArtistResponse>(json);

        //        if (responseData == null)
        //        {
        //            throw new Exception("Error getting data");
        //        }

        //        return responseData;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Failed to get data from API: {ex.Message}");
        //    }
        //}
    }
}
