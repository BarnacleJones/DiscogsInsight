using DiscogsInsight.ApiIntegration.Contract.DiscogsResponseModels;
using DiscogsInsight.ApiIntegration.Contract.MusicBrainzResponseModels;

namespace DiscogsInsight.ApiIntegration.Contract.Services
{
    public interface IMusicBrainzApiService
    {
        Task<MusicBrainzInitialArtist> GetInitialArtistFromMusicBrainzApi(string artistName);
        Task<MusicBrainzArtist> GetArtistFromMusicBrainzApiUsingArtistId(string musicBrainzArtistId);
        Task<MusicBrainzRelease> GetReleaseFromMusicBrainzApiUsingMusicBrainsReleaseId(string musicBrainzReleaseId);
        Task<MusicBrainzReleaseGroup> GetReleaseGroupFromMusicBrainzApiUsingMusicBrainsReleaseId(string musicBrainzReleaseId);
    }
}
