﻿using DiscogsInsight.DataAccess.Interfaces;
using SQLite;

namespace DiscogsInsight.DataAccess.Entities
{
    public class MusicBrainzArtistToMusicBrainzRelease : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        //public int TagId {  get; set; }
        public string? MusicBrainzArtistId { get; set;}
        public string? MusicBrainzReleaseId { get; set;}
        public bool IsAReleaseGroupGroupId { get; set;}
        public string? MusicBrainzReleaseName { get; set;}
        public string? ReleaseYear { get; set;}
        public int DiscogsArtistId { get; set;}
        public string? Status { get; set;}//official, bootleg etc

    }
}
