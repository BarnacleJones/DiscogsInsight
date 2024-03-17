using DiscogsInsight.DataAccess.Interfaces;
using SQLite;

namespace DiscogsInsight.DataAccess.Entities
{
    public class MusicBrainzTags : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }        
        public string? Tag {  get; set; }
    }
}
