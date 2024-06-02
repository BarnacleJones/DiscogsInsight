using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface IReleaseDataService
    {
        Task<bool> SetFavouriteBooleanOnRelease(bool favourited, int discogsReleaseId);
        Task<List<Release>> GetAllReleasesAsList();
        Task<List<int?>> GetAllDiscogsReleaseIdsForArtist(int? discogsArtistId);
        Task<(Release?, byte[]?)> GetReleaseAndImageAndRetrieveAllApiDataForRelease(int? discogsReleaseId);
        Task<byte[]?> GetImageForRelease(string musicBrainzReleaseId);
        Task<(Release?, byte[]?)> GetRandomRelease();
        Task<List<Release>> GetNewestReleases(int howManyToReturn);
        Task<List<PossibleReleasesFromArtist>> GetPossibleReleasesForDataCorrectionFromDiscogsReleaseId(int? discogsReleaseId);
        Task<bool> UpdateReleaseToBeNewMusicBrainzReleaseId(int? discogsReleaseId, string musicBrainzReleaseId);
    }

    public class PossibleReleasesFromArtist
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string MusicBrainzReleaseId { get; set; }
    }
}
