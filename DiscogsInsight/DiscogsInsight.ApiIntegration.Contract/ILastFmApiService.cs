using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Scrobblers;

namespace DiscogsInsight.ApiIntegration.Contract
{
    public interface ILastFmApiService
    {
        public Task<LastResponse> ScrobbleRelease(Scrobble scrobble);
        public Task<LastResponse> ScrobbleReleases(List<Scrobble> scrobbles);
        public Task<LastAlbum> GetAlbumInformation(string artistName, string albumName);
    }
}
