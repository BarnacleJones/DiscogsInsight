﻿using DiscogsInsight.ApiIntegration.Models.DiscogsResponseModels;
using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface IDiscogsGenresAndTagsDataService
    {
        Task<List<DiscogsGenreTags>> GetAllGenreTagsAsList();
        Task<List<DiscogsGenreTagToDiscogsRelease>> GetDiscogsGenreTagToDiscogsReleaseAsList();
        Task<List<(string?, int)>> GetGenresForDiscogsRelease(int? discogsReleaseId);
        Task<bool> SaveStylesFromDiscogsRelease(DiscogsReleaseResponse releaseResponse, int discogsReleaseId, int discogsArtistId);
        Task<bool> SaveGenresFromDiscogsRelease(ResponseRelease responseRelease, int? discogsReleaseId, int? discogsArtistId);
    }
}