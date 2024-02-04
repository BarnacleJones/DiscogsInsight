using DiscogsInsight.DataModels;
using DiscogsInsight.ResponseModels;
using Newtonsoft.Json;

namespace DiscogsInsight.Services
{
    public class DiscogsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly DiscogsInsightDb _db;
        private string _discogsUserName;
        public DiscogsApiService(HttpClient httpClient, DiscogsInsightDb db)
        {
            _httpClient = httpClient;
            _db = db;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DiscogsInsight");
            _discogsUserName = Preferences.Default.Get("discogsUsername", "Unknown");
        }

        public async Task<List<Release>> GetCollectionFromDiscogsAndSaveAndReturn()
        {
            //https://json2csharp.com/ - put in the json response, generates lots of classes, split them up

            var collectionUrl = $"https://api.discogs.com/users/{_discogsUserName}/collection/releases/0?page=1&per_page=1000";

            var response = await _httpClient.GetAsync(collectionUrl);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<DiscogsCollectionResponse>(json);
                if (responseData is null)
                    throw new Exception("Error getting data");
                var success = await _db.SaveDiscogsCollectionResponse(responseData);
                if (success)
                {
                    return await _db.GetAllEntitiesAsync<Release>();
                }
            }

            throw new Exception("Failed to get data from API: " + response.ReasonPhrase);
        }
      
    }
}
