using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface IReleaseDataService
    {
        Task SetFavouriteBooleanOnRelease(bool favourited, int discogsReleaseId);
        Task<List<Release>> GetAllReleasesAsList();
        Task<List<PossibleReleasesFromArtist>> GetPossibleReleasesForDataCorrectionFromDiscogsReleaseId(int? discogsReleaseId);
        Task<bool> UpdateReleaseToBeNewMusicBrainzReleaseId(int? discogsReleaseId, string musicBrainzReleaseId);
        Task<List<ReleaseDataModel>> GetReleaseDataModelsByDiscogsGenreTagId(int discogsGenreTagId);
        Task<ReleaseDataModel> GetReleaseDataModelByDiscogsReleaseId(int discogsReleaseId);
        Task<List<ReleaseDataModel>> GetNewestReleases(int howManyToReturn);
        Task<List<ReleaseDataModel>> GetAllReleaseDataModelsForArtist(int howManyToReturn);
        Task<List<ReleaseDataModel>> GetAllReleaseDataModelsByYear(int year);
        Task<ReleaseDataModel> GetRandomRelease();
        Task<string> ScrobbleRelease(int discogsReleaseId);
    }
}
