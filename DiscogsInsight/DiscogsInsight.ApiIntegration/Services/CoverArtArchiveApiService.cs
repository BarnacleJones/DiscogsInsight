using DiscogsInsight.ApiIntegration.MusicBrainzResponseModels;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DiscogsInsight.ApiIntegration.Services
{
    public class CoverArtArchiveApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CoverArtArchiveApiService> _logger;

        //-----------------------------------------------------------------------------
        //Hard coded MusicBrainz Cover Art API Endpoints. If they ever change this should be the only place they are referenced!

        //MusicBrainz Cover Art Api documentation: https://musicbrainz.org/doc/Cover_Art_Archive/API

        //Cover images, just add the release id - note NOT the release-group id

        private const string ImageDataByReleaseUrl = "https://coverartarchive.org/release/";
        private const string ImageDataByReleaseGroupUrl = "https://coverartarchive.org/release-group/";

        //-----------------------------------------------------------------------------


        public CoverArtArchiveApiService(IHttpClientFactory httpClientFactory, ILogger<CoverArtArchiveApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("CoverArtApiClient");
                     
            _logger = logger;            
        }

        public async Task<MusicBrainzCover> GetCoverResponseByMusicBrainzReleaseId(string musicBrainzReleaseId, bool isAReleaseGroupId)
        {
            try
            {
                var responseData = new MusicBrainzCover();

                var fullArtistRequestUrl = isAReleaseGroupId ? ImageDataByReleaseGroupUrl + musicBrainzReleaseId  : ImageDataByReleaseUrl + musicBrainzReleaseId;
                
                var response = await _httpClient.GetAsync(fullArtistRequestUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                responseData = JsonSerializer.Deserialize<MusicBrainzCover>(json);
                if (responseData == null)
                        throw new Exception("Error getting musicbrainz cover data");

                return responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get data from API");

                throw new Exception("Issue retrieving cover art for album.");
            }
        }
        
       
        public async Task<byte[]> GetCoverByteArray(string? coverUrl)
        {
            try
            {
                var fullArtistRequestUrl = coverUrl;
                //need to make the urls https for android to work!
                //its either do it here or at the saving of the url into the db
                //...but ill do it here for those already saved in the db
                if (fullArtistRequestUrl.StartsWith("http://"))
                {
                    fullArtistRequestUrl = "https://" + fullArtistRequestUrl.Substring(7); // Skip the "http://"
                }

                var response = await _httpClient.GetAsync(fullArtistRequestUrl);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadAsByteArrayAsync();
                //var json = await response.Content.ReadAsStringAsync();
                //var responseData = JsonConvert.DeserializeAnonymousType<byte[]>(json);

                if (responseData == null)
                    throw new Exception("Error getting musicbrainz cover data");

                return responseData;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cover buyte array from API");
                throw;
            }
        }
    }
}
