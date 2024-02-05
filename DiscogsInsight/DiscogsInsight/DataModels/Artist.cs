using SQLite;

namespace DiscogsInsight.DataModels
{
    public class Artist : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int DiscogsArtistId { get; set; }//Foreign key
        public string Name { get; set; }
        public string ResourceUrl { get; set; }
    }
}
