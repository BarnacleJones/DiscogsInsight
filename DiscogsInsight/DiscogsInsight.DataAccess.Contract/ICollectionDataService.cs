using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface ICollectionDataService
    {
        Task<List<DiscogsArtistIdAndName>> GetArtistsIdsAndNames();
        Task<List<Release>> GetReleases();
        Task<bool> UpdateCollectionFromDiscogs();
        Task<bool> CheckCollectionIsSeededOrSeed();
    }
}
