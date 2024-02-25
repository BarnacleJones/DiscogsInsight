using DiscogsInsight.DataAccess.Interfaces;
using SQLite;

namespace DiscogsInsight.DataAccess.Entities
{
    public class Artist : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int? DiscogsArtistId { get; set; }//Foreign key
        public string? Name { get; set; }
        public string? Profile { get; set; }
    }
}
