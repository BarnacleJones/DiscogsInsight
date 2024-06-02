using DiscogsInsight.DataAccess.Models;
using Artist = DiscogsInsight.Database.Entities.Artist;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface IArtistDataService
    {
        Task<Artist?> GetArtistByDiscogsId(int discogsArtistId);
        Task<List<Artist>?> GetArtists();
        Task<Artist?> GetArtist(int? discogsArtistId, bool fetchAndSaveApiData = true);
        Task<List<MusicBrainzArtistRelease>> GetArtistsReleasesByMusicBrainzArtistId(string musicBrainzArtistId);
        Task<List<PossibleArtistsFromMusicBrainzApi>> GetPossibleArtistsForDataCorrectionFromDiscogsReleaseId(int? discogsReleaseId);
        Task<bool> DeleteExistingArtistDataAndUpdateToChosenMusicBrainzArtistFromMusicBrainzId(int? discogsReleaseId, string newAritstMusicBrainzId);
    }

    public class PossibleArtistsFromMusicBrainzApi
    {
        public string CorrectArtistMusicBrainzId { get; set; }
        public string ArtistName { get; set; }
        public string Country { get; set; }
        public string Disambiguation { get; set; }
    }
}
