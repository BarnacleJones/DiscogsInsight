using DiscogsInsight.DataAccess.Models;
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
        Task<(string, List<PossibleReleasesFromArtist>)> GetAllStoredMusicBrainzReleasesForArtistByDiscogsReleaseId(int? discogsReleaseId);
        Task<bool> UpdateReleaseToBeNewMusicBrainzReleaseId(int? discogsReleaseId, string musicBrainzReleaseId);
    }
}
