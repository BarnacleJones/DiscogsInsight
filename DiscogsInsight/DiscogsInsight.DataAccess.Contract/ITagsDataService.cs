using DiscogsInsight.ApiIntegration.Models.MusicBrainzResponseModels;
using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.DataAccess.Contract
{
    public interface ITagsDataService
    {
        Task<bool> SaveTagsByMusicBrainzArtistId(MusicBrainzInitialArtist artistResponse, string musicBrainzArtistId);
        Task<List<MusicBrainzTags>> GetTagsByMusicBrainzArtistId(string musicBrainzArtistId);
    }
}
