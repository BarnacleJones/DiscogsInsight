using DiscogsInsight.DataAccess.Models;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface IArtistDataCorrectionService
    {
        Task<List<PossibleArtistsFromMusicBrainzApi>> GetPossibleArtistsForDataCorrectionFromDiscogsReleaseId(int? discogsReleaseId);
        Task DeleteExistingArtistDataAndUpdateToChosenMusicBrainzArtistFromMusicBrainzId(int? discogsReleaseId, string newAritstMusicBrainzId);
    }
}
