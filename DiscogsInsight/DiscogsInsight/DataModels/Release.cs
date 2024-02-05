using SQLite;

namespace DiscogsInsight.DataModels
{
    public class Release : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int? ArtistId { get; set; }//DiscogssArtistId on Artist - FK
        public int DiscogsReleaseId { get; set; }
        public int DiscogsMasterId { get; set; }
        public string Title { get; set; }
        public DateTime DateAdded { get; set; }
        public string ResourceUrl { get; set; }
        public string MasterUrl { get; set; }
        public string Genres {get;set;}      
        public int Year { get; set; }
    }
}
