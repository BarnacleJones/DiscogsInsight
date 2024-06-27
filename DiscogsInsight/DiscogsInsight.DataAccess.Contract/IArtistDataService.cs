using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface IArtistDataService
    {
        Task<int?> GetARandomDiscogsArtistId();
        Task<Artist?> GetArtistByDiscogsId(int? discogsArtistId, bool fetchAndSaveApiData = true);
        Task<List<MusicBrainzArtistRelease>> GetArtistsReleasesByMusicBrainzArtistId(string musicBrainzArtistId);
       
    }
}
