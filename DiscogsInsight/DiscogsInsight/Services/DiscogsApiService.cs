using DiscogsInsight.Models;
using DiscogsInsight.ResponseModels;
using Newtonsoft.Json;

namespace DiscogsInsight.Services
{
    public class DiscogsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly DiscogsInsightDb _db;
        private string _discogsUserName;
        public DiscogsApiService(HttpClient httpClient, DiscogsInsightDb dbContext)
        {
            _httpClient = httpClient;
            _db = dbContext;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DiscogsInsight");
            _discogsUserName = Preferences.Default.Get("discogsUsername", "Unknown");
        }

        public async Task<DiscogsCollectionResponse> GetApiDataFromDiscogs()
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
                return responseData;
            }

            throw new Exception("Failed to get data from API: " + response.ReasonPhrase);
        }
        public async Task<DiscogsCollectionResponse> GetCollection()
        {
            var data = await GetApiDataFromDiscogs();

            if (data == null) return await Task.FromResult(data);

            foreach (var release in data.releases)
            {
                //create a seeding service
                //do each table
                var artists = release.basic_information.artists.Select(x => new Artist
                {
                    DiscogsArtistId = x.id,
                    Name = x.name,
                    ResourceUrl = x.resource_url
                });
                foreach (var artist in artists)
                {
                    await _db.SaveItemAsync(artist);
                }
            }

            return await Task.FromResult(data);
        }
    }
}
