﻿using DiscogsInsight.Database.Contract;
using SQLite;

namespace DiscogsInsight.Database.Entities
{
    public class DiscogsGenreTagToDiscogsRelease : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int? DiscogsGenreTagId { get; set; } //Referred to as GenreId, todo: try to phase that out
        public int? DiscogsArtistId { get; set; }//DiscogssArtistId on Artist - FK
        public int? DiscogsReleaseId { get; set; }
    }
}
