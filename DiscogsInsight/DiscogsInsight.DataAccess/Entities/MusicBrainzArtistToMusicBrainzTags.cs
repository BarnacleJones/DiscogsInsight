using DiscogsInsight.DataAccess.Contract;
using SQLite;

namespace DiscogsInsight.DataAccess.Entities
{
    public class MusicBrainzArtistToMusicBrainzTags : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }        
        public int TagId {  get; set; }
        public string? MusicBrainzArtistId { get; set;}
    }
}
