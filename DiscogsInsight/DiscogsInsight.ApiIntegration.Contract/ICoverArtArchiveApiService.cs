using DiscogsInsight.ApiIntegration.Models.MusicBrainzResponseModels;

namespace DiscogsInsight.ApiIntegration.Contract
{
    public interface ICoverArtArchiveApiService
    {
        Task<MusicBrainzCover> GetCoverResponseByMusicBrainzReleaseId(string musicBrainzReleaseId, bool isAReleaseGroupId);
        Task<byte[]> GetCoverByteArray(string? coverUrl);
    }
}
