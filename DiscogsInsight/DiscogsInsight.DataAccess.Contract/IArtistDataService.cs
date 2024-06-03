using DiscogsInsight.DataAccess.Models;
using Artist = DiscogsInsight.Database.Entities.Artist;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface IArtistDataService
    {
        //Task<Artist?> GetArtistByDiscogsIdNoApiChecks(int discogsArtistId);
        Task<List<Artist>?> GetAllArtistsFromDatabase();
        Task<Artist?> GetArtistByDiscogsId(int? discogsArtistId, bool fetchAndSaveApiData = true);
        Task<List<MusicBrainzArtistRelease>> GetArtistsReleasesByMusicBrainzArtistId(string musicBrainzArtistId);
        Task<List<PossibleArtistsFromMusicBrainzApi>> GetPossibleArtistsForDataCorrectionFromDiscogsReleaseId(int? discogsReleaseId);
        Task<bool> DeleteExistingArtistDataAndUpdateToChosenMusicBrainzArtistFromMusicBrainzId(int? discogsReleaseId, string newAritstMusicBrainzId);
    }
}
