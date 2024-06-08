using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface IReleaseDataService
    {
        Task SetFavouriteBooleanOnRelease(bool favourited, int discogsReleaseId);
        Task<List<Release>> GetAllReleasesAsList();

        //data correction
        Task<List<PossibleReleasesFromArtist>> GetPossibleReleasesForDataCorrectionFromDiscogsReleaseId(int? discogsReleaseId);
        Task<(string, List<PossibleReleasesFromArtist>)> GetAllStoredMusicBrainzReleasesForArtistByDiscogsReleaseId(int? discogsReleaseId);
        Task<bool> UpdateReleaseToBeNewMusicBrainzReleaseId(int? discogsReleaseId, string musicBrainzReleaseId);

        //new
        Task<List<ReleaseDataModel>> GetReleaseDataModelsByDiscogsGenreTagId(int discogsGenreTagId);
        Task<ReleaseDataModel> GetReleaseDataModelByDiscogsReleaseId(int discogsReleaseId);
        Task<List<ReleaseDataModel>> GetNewestReleases(int howManyToReturn);
        Task<List<ReleaseDataModel>> GetAllReleaseDataModelsForArtist(int howManyToReturn);
        Task<List<ReleaseDataModel>> GetAllReleaseDataModelsByYear(int year);
        Task<ReleaseDataModel> GetRandomRelease();
    }
}
