using DiscogsInsight.DataAccess.Interfaces;
using SQLite;

namespace DiscogsInsight.DataAccess.Entities
{
    public class Release : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int? DiscogsArtistId { get; set; }//DiscogssArtistId on Artist - FK
        public int? DiscogsReleaseId { get; set; }
        public int? DiscogsMasterId { get; set; }
        public string? Title { get; set; }
        public DateTime? DateAdded { get; set; }
        public string? Genres { get; set; }
        public int? Year { get; set; }
        public string? ReleaseCountry { get; set; }
        public string? MusicBrainzReleaseId { get; set; }
        public string? MusicBrainzCoverUrl { get; set; }
        public byte[]? MusicBrainzCoverImage { get; set; }
    }
}
