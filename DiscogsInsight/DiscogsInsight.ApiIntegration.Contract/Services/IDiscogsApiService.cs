using DiscogsInsight.ApiIntegration.Contract.DiscogsResponseModels;

namespace DiscogsInsight.ApiIntegration.Contract.Services
{
    public interface IDiscogsApiService
    {
        Task<DiscogsCollectionResponse> GetCollectionFromDiscogsApi();
        Task<DiscogsReleaseResponse> GetReleaseFromDiscogs(int discogsReleaseId);
        Task<DiscogsArtistResponse> GetArtistFromDiscogs(int discogsArtistId);
    }
}
