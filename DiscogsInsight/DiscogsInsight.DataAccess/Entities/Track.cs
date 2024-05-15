﻿using DiscogsInsight.DataAccess.Contract;
using SQLite;

namespace DiscogsInsight.DataAccess.Entities
{
    public class Track : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int? DiscogsArtistId { get; set; }//DiscogssArtistId on Artist - FK
        public int? DiscogsReleaseId { get; set; }
        public int? DiscogsMasterId { get; set; }
        public string? Title { get; set; }
        public string? Duration { get; set; }
        public long? MusicBrainzTrackLength { get; set; }
        public string? Position { get; set; }//Eg A1, A2, B1, etc - sorting property
        public int? Rating { get; set; }
    }
}
