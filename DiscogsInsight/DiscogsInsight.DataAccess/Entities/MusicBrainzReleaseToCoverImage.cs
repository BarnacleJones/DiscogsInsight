using DiscogsInsight.DataAccess.Interfaces;
using SQLite;

namespace DiscogsInsight.DataAccess.Entities
{
    public class MusicBrainzReleaseToCoverImage : IDatabaseEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string? MusicBrainzReleaseId { get; set;}

        public byte[]? MusicBrainzCoverImage { get; set; }
    }
}
