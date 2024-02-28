using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DiscogsInsight.ApiIntegration.Services
{
    public class MusicBrainzApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MusicBrainzApiService> _logger;

        //-----------------------------------------------------------------------------
        //Hard coded MusicBrainz API Endpoints. If they ever change this should be the only place they are referenced!

        //MusicBrainzApi documentation: https://musicbrainz.org/doc/MusicBrainz_API


        private const string InitialArtistRequest = "http://musicbrainz.org/ws/2/release-group/?query=artist:\"michael jackson\"";

        //know about release groups - thats what you want at this stage, its the main release info https://musicbrainz.org/doc/Release_Group

        private const string ReleaseGroupUrl = "http://musicbrainz.org/ws/2/release-group/940a8468-73dd-4c0c-94a8-823b1b13c736";
        private const string ReleaseGroupIncludeUrl = "?inc=artists+releases";

        private const string ReleaseUrl = "http://musicbrainz.org/ws/2/release/59211ea4-ffd2-4ad9-9a4e-941d3148024a";
        private const string ReleaseIncludeUrl = "?inc=artist-credits+labels+discids+recordings+tags";

        private const string ArtistUrl = "http://musicbrainz.org/ws/2/artist/b574bfea-2359-4e9d-93f6-71c3c9a2a4f0";
        private const string ArtistIncludeUrl = "?inc=aliases+releases";

        //-----------------------------------------------------------------------------


        public MusicBrainzApiService(HttpClient httpClient, ILogger<MusicBrainzApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DiscogsInsight");
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
                
                var collectionUrl = $"https://api.discogs.com/users/collection/releases/0?page={currentPage}&per_page=1000";//500 is max but hey

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
