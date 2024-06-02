using DiscogsInsight.ApiIntegration.Models.DiscogsResponseModels;

namespace DiscogsInsight.ApiIntegration.Contract
{
    public interface IDiscogsApiService
    {
        Task<DiscogsCollectionResponse> GetCollectionFromDiscogsApi();
        Task<DiscogsReleaseResponse> GetReleaseFromDiscogs(int discogsReleaseId);
        Task<DiscogsArtistResponse> GetArtistFromDiscogs(int discogsArtistId);
    }
}
