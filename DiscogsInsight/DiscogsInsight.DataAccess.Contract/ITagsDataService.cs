using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface ITagsDataService
    {
        Task<List<MusicBrainzTags>> GetTagsByMusicBrainzArtistId(string musicBrainzArtistId);
    }
}
