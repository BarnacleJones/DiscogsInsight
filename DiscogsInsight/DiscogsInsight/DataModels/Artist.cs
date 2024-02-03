using SQLite;

namespace DiscogsInsight.Models
{
    public class Artist : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int DiscogsArtistId { get; set; }
        public string Name { get; set; }
        public string ResourceUrl { get; set; }
    }
}
