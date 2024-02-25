using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DiscogsInsight.ApiIntegration.Services
{
    public class DiscogsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DiscogsApiService> _logger;
        private string _discogsUserName;

        //-----------------------------------------------------------------------------
        //Hard coded Disgogs API Endpoints. If they ever change this should be the only place they are referenced!

        //If they take a parameter (Eg ArtistId, use value from entity prefixed with Discogs eg. DiscogsArtistId

        private const string ReleaseUrl = "https://api.discogs.com/releases/";
        private const string MasterReleaseUrl = "https://api.discogs.com/masters/";
        private const string ArtistUrl = "https://api.discogs.com/artists/";

        //-----------------------------------------------------------------------------


        public DiscogsApiService(HttpClient httpClient, ILogger<DiscogsApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DiscogsInsight");
            _discogsUserName = Preferences.Default.Get("discogsUsername", "Unknown");
        }

        public async Task<DiscogsCollectionResponse> GetCollectionFromDiscogsApi()
        {
            try
            {
                //API can handle 25 requests per minute - if pages are over that it will not work
                //todo: create ApiOverloadException and throw in that case

                var releases = new List<ResponseRelease>();
                var currentPage = 1;
                var totalPages = 1;
                var responseData = new DiscogsCollectionResponse();
                _discogsUserName = Preferences.Default.Get("discogsUsername", "Unknown");
                if (string.IsNullOrEmpty(_discogsUserName))
                {
                    _logger.LogError("Empty username");
                    return new DiscogsCollectionResponse();
                }
                var collectionUrl = $"https://api.discogs.com/users/{_discogsUserName}/collection/releases/0?page={currentPage}&per_page=1000";//500 is max but hey

                do
                {
                    var response = await _httpClient.GetAsync(collectionUrl);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    responseData = JsonConvert.DeserializeObject<DiscogsCollectionResponse>(json);

                    if (responseData == null)
                         throw new Exception("Error getting data");
                    if(responseData.releases == null) //no releases in collection
                        return new DiscogsCollectionResponse();

                    releases.AddRange(responseData.releases);

                    totalPages = responseData?.pagination?.pages ?? 1;
                    currentPage++;

                } while (currentPage <= totalPages);

                if (releases is null)
                    throw new Exception("Error getting data");

                return new DiscogsCollectionResponse { releases = releases };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get data from API: {ex.Message}");
            }
        }
        public async Task<DiscogsReleaseResponse> GetReleaseFromDiscogs(int discogsReleaseId)
        {
            try
            {
                var thisReleaseUrl = ReleaseUrl + discogsReleaseId;
                var response = await _httpClient.GetAsync(thisReleaseUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<DiscogsReleaseResponse>(json);

                if (responseData == null)
                {
                    throw new Exception("Error getting data");
                }

                return responseData;

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get data from API: {ex.Message}");
            }
        }
        public async Task<DiscogsArtistResponse> GetArtistFromDiscogs(int discogsArtistId)
        {
            try
            {
                var thisArtistUrl = ArtistUrl + discogsArtistId;
                var response = await _httpClient.GetAsync(thisArtistUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<DiscogsArtistResponse>(json);

                if (responseData == null)
                {
                    throw new Exception("Error getting data");
                }

                return responseData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get data from API: {ex.Message}");
            }
        }
    }
}
