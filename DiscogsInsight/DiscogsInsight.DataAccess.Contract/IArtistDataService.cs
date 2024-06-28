using DiscogsInsight.DataAccess.Models;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface IArtistDataService
    {
        Task<int?> GetARandomDiscogsArtistId();
        Task<ArtistDataModel?> GetArtistDataModelByDiscogsId(int? discogsArtistId);
    }
}
