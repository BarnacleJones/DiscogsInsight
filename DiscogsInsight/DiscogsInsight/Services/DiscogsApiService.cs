using DiscogsInsight.DataModels;
using DiscogsInsight.ResponseModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DiscogsInsight.Services
{
    public class DiscogsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly CollectionDataService _db; 
        private readonly ILogger<DiscogsApiService> _logger;
        private string _discogsUserName;

        //-----------------------------------------------------------------------------
        //Hard coded Disgogs API Endpoints. If they ever change this should be the only place they are referenced!

        //If they take a parameter (Eg ArtistId, use value from entity prefixed with Discogs eg. DiscogsArtistId

        private const string ReleaseUrl = "https://api.discogs.com/releases/";
        private const string MasterReleaseUrl = "https://api.discogs.com/masters/";
        private const string ArtistUrl = "https://api.discogs.com/artists/";

        //-----------------------------------------------------------------------------


        public DiscogsApiService(HttpClient httpClient, CollectionDataService db, ILogger<DiscogsApiService> logger)
        {
            _httpClient = httpClient;
            _db = db;
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DiscogsInsight");
            _discogsUserName = Preferences.Default.Get("discogsUsername", "Unknown");
        }

        public async Task<List<Release>> GetCollectionFromDiscogsAndSaveAndReturn()
        {
            try
            {
                //API can handle 25 requests per minute - if pages are over that it will most likely not work

                var releases = new List<ResponseRelease>();
                var currentPage = 1;
                var totalPages = 1;
                var responseData = new DiscogsCollectionResponse();
                _discogsUserName = Preferences.Default.Get("discogsUsername", "Unknown");
                if (string.IsNullOrEmpty(_discogsUserName))
                {
                    _logger.LogError("Empty username");
                    return new List<Release>();
                }
                var collectionUrl = $"https://api.discogs.com/users/{_discogsUserName}/collection/releases/0?page={currentPage}&per_page=1000";//500 is max but hey
                
                do
                {
                    var response = await _httpClient.GetAsync(collectionUrl);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    responseData = JsonConvert.DeserializeObject<DiscogsCollectionResponse>(json);

                    if (responseData == null)
                    {
                        throw new Exception("Error getting data");
                    }

                    releases.AddRange(responseData.releases);

                    totalPages = responseData.pagination.pages;
                    currentPage++;

                } while (currentPage <= totalPages);
               
                if (releases is null)
                    throw new Exception("Error getting data");

                var data = new DiscogsCollectionResponse { releases = releases};

                var success = await _db.SaveDiscogsCollectionResponse(data);
                if (!success)
                {
                    throw new Exception("Error saving collection data.");
                }
                return await _db.GetAllEntitiesAsync<Release>();
            }
            catch(Exception ex) { 
            
                throw new Exception($"Failed to get data from API: {ex.Message}");
            }
        }
        public async Task<bool> GetReleaseFromDiscogsAndSave(int discogsReleaseId)
        {
            try
            {
                var thisReleaseUrl = ReleaseUrl + discogsReleaseId;
                var response = await _httpClient.GetAsync(thisReleaseUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject <DiscogsReleaseResponse>(json);

                if (responseData == null)
                {
                    throw new Exception("Error getting data");
                }

                var success = await _db.SaveDiscogsReleaseResponse(responseData);

                if (!success)
                {
                    throw new Exception("Error saving release data.");
                }
                return true;
            }
            catch(Exception ex) { 
            
                throw new Exception($"Failed to get data from API: {ex.Message}");
            }
        }

        public async Task<bool> GetArtistFromDiscogsAndSave(int discogsArtistId)
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

                var success = await _db.SaveDiscogsArtistResponse(responseData);

                if (!success)
                {
                    throw new Exception("Error saving Artist data.");
                }
                return true;
            }
            catch (Exception ex)
            {

                throw new Exception($"Failed to get data from API: {ex.Message}");
            }
        }
    }
}
