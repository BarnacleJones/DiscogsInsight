using DiscogsInsight.ApiIntegration.Models.MusicBrainzResponseModels;

namespace DiscogsInsight.ApiIntegration.Contract
{
    public interface IMusicBrainzApiService
    {
        Task<MusicBrainzInitialArtist> GetInitialArtistFromMusicBrainzApi(string artistName);
        Task<MusicBrainzArtist> GetArtistFromMusicBrainzApiUsingArtistId(string musicBrainzArtistId);
        Task<MusicBrainzRelease> GetReleaseFromMusicBrainzApiUsingMusicBrainsReleaseId(string musicBrainzReleaseId);
        Task<MusicBrainzReleaseGroup> GetReleaseGroupFromMusicBrainzApiUsingMusicBrainsReleaseId(string musicBrainzReleaseId);
    }
}
