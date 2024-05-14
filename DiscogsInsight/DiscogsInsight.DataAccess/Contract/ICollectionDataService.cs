﻿using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.DataAccess.Entities;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface ICollectionDataService
    {
        Task<List<DiscogsArtistIdAndName>> GetArtistsIdsAndNames();
        Task<List<Release>> GetReleases();
        Task PurgeEntireCollection();
        Task PurgeEntireDatabase();
        Task<bool> CollectionSavedOrUpdatedFromDiscogs();
    }
}
