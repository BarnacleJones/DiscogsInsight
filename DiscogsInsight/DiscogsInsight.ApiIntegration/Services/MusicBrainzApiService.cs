using DiscogsInsight.ApiIntegration.MusicBrainzResponseModels;
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


        //private const string InitialArtistRequest = "/release-group/?query=artist:\"michael jackson\"";
        private const string InitialArtistRequest = "/ws/2/artist/?query=artist:";
        private const string InitialArtistIncludeUrl = "?inc=aliases";

        //know about release groups - thats what you want at this stage, its the main release info https://musicbrainz.org/doc/Release_Group

        private const string ReleaseGroupUrl = "/ws/2/release-group/940a8468-73dd-4c0c-94a8-823b1b13c736";
        private const string ReleaseGroupIncludeUrl = "?inc=artists+releases";

        private const string ReleaseUrl = "/ws/2/release/59211ea4-ffd2-4ad9-9a4e-941d3148024a";
        private const string ReleaseIncludeUrl = "?inc=artist-credits+labels+discids+recordings+tags";

        private const string ArtistUrl = "/ws/2/artist/";
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

                var fullArtistRequestUrl = InitialArtistRequest + $"'{artistName}'";
                
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
        public async Task<MusicBrainzArtist> GetArtistFromMusicBrainzApiUsingArtistId(string musicBrainzArtistId)
        {
            //not using at this stage - really only has additional release data (from the query string) but not displaying all releases by artist at this point
            //todo: maybe a button on the artist page to view all releases by artist, dont even save them? just bring a list to the view
            try
            {
                var responseData = new MusicBrainzArtist();

                var fullArtistRequestUrl = ArtistUrl + $"{musicBrainzArtistId}" + ArtistIncludeUrl;
                
                var response = await _httpClient.GetAsync(fullArtistRequestUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                responseData = JsonConvert.DeserializeObject<MusicBrainzArtist>(json);

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

        public async Task<(string ReleaseId, string CoverArtUrl)> GetReleaseIdAndCoverArtUrlFromMusicBrainzApiByReleaseTitle(string title, int discogsArtistId, string musicBrainzArtistId)
        {
            try
            {
                //get artist releases by getting the artists name or musicbrainzid
                var artist = await GetArtistFromMusicBrainzApiUsingArtistId(musicBrainzArtistId);
                //save those releases in new table musicbrainzartisttomusicbrainzreleases
                //save the release name and the release id, for showing releases on artists page and getting release cover art by id

                //then go through the artists releases and look for the title that matches variable title
                var releaseIdForThisRelease = artist.Releases.Where(x => x.Title == title).FirstOrDefault();//will need regex
                //use that release id to get cover art url - deserialise to MusicBrainzCover
                var coverUrl = GetCoverArtUrlFromMusicBrainzApiByMusicBrainzReleaseId(releaseIdForThisRelease.Id);
                //just return the two values and it is saved in the data service for release

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get data from API");
                throw;
            }
        }
        private async Task<string> GetCoverArtUrlFromMusicBrainzApiByMusicBrainzReleaseId(string musicBrainzReleaseId)
        {
            return "";
        }
    }
}
