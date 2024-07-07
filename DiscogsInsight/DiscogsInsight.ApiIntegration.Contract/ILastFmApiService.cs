using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

namespace DiscogsInsight.ApiIntegration.Contract
{
    public interface ILastFmApiService
    {
        public Task<LastResponse> ScrobbleRelease(Scrobble scrobble);
        public Task<LastResponse> ScrobbleReleases(List<Scrobble> scrobbles);
        public Task<LastAlbumResponseManual> GetAlbumInformation(string artistName, string albumName);
    }
}
