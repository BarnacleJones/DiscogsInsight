﻿using DiscogsInsight.Database.Contract;
using SQLite;

namespace DiscogsInsight.Database.Entities
{
    public class Artist : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int? DiscogsArtistId { get; set; }//Foreign key
        public string? Name { get; set; }
        public string? Profile { get; set; }

        //musicBrainz data
        public string? MusicBrainzArtistId { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? StartYear { get; set; }
        public string? EndYear { get; set; }
        public bool HasAllApiData { get; set; }
    }
}
