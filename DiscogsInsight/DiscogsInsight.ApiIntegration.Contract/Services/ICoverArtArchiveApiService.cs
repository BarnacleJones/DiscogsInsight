using DiscogsInsight.ApiIntegration.Contract.MusicBrainzResponseModels;

namespace DiscogsInsight.ApiIntegration.Contract.Services
{
    public interface ICoverArtArchiveApiService
    {
        Task<MusicBrainzCover> GetCoverResponseByMusicBrainzReleaseId(string musicBrainzReleaseId, bool isAReleaseGroupId);
        Task<byte[]> GetCoverByteArray(string? coverUrl);
    }
}
