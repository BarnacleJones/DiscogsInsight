using DiscogsInsight.Database.Contract;
using SQLite;

namespace DiscogsInsight.Database.Entities
{
    public class MusicBrainzReleaseToCoverImage : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }        
        public string? MusicBrainzReleaseId { get; set;}
        public byte[]? MusicBrainzCoverImage { get; set; }
    }
}
