using DiscogsInsight.Database.Contract;
using SQLite;

namespace DiscogsInsight.Database.Entities
{
    public class MusicBrainzTags : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }        
        public string? Tag {  get; set; }
    }
}
