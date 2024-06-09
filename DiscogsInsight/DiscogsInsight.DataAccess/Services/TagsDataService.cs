using DiscogsInsight.ApiIntegration.Models.MusicBrainzResponseModels;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.Database.Contract;
using DiscogsInsight.Database.Entities;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.DataAccess.Services
{
    public class TagsDataService : ITagsDataService
    {
        private readonly ISQLiteAsyncConnection _db;
        private readonly ILogger<TagsDataService> _logger;

        public TagsDataService(ISQLiteAsyncConnection db, ILogger<TagsDataService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<bool> SaveTagsByMusicBrainzArtistId(MusicBrainzInitialArtist artistResponse, string musicBrainzArtistId)
        {
            try
            {
                var tagsTable = await _db.Table<MusicBrainzTags>().ToListAsync();
                var existingTagNamesInDb = tagsTable.Any() ? tagsTable.Select(x => x.Tag).ToList() : new List<string?>();

                var artistFromResponse =   artistResponse.Artists.Where(x => x.Id == musicBrainzArtistId).FirstOrDefault();
                if (artistFromResponse == null) { return true;  } //artist id mismatch potentially
                var tagsInResponse = artistFromResponse.Tags == null ? new List<Tag>() : artistFromResponse.Tags.Where(x => x.Count >= 1).ToList();
                     
                if (tagsInResponse.Any()) 
                {
                    foreach (var tag in tagsInResponse)
                    {
                        if (!existingTagNamesInDb.Contains(tag.Name))
                        {
                            var tagToSave = new MusicBrainzTags { Tag = tag.Name };
                            await _db.InsertAsync(tagToSave);
                        }
                    }                
                }

                //get table again with newly saved tags to save to joining table
                tagsTable = await _db.Table<MusicBrainzTags>().ToListAsync();
                
                foreach (var tag in tagsInResponse)
                {
                    var tagId = tagsTable.Where(x => x.Tag == tag.Name).FirstOrDefault()?.Id;
                    if (tagId != null)
                    {
                        var tagToArtist = new MusicBrainzArtistToMusicBrainzTags { TagId = tagId.Value, MusicBrainzArtistId = musicBrainzArtistId };
                        await _db.InsertAsync(tagToArtist);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at SaveTagsByMusicBrainzArtistId:{ex.Message} ");
                throw;
            }
        }

        public async Task<List<MusicBrainzTags>> GetTagsByMusicBrainzArtistId(string musicBrainzArtistId)
        {
            var musicBrainzTagsList = await _db.Table<MusicBrainzTags>().ToListAsync();
            var musicBrainzTagsToArtistsTable = await _db.Table<MusicBrainzArtistToMusicBrainzTags>().ToListAsync();

            var musicBrainzTagsToArtistsList = musicBrainzTagsToArtistsTable.Where(x => x.MusicBrainzArtistId == musicBrainzArtistId);
            var tagsIdsListForArtist =  musicBrainzTagsToArtistsList.Select(x => x.TagId).ToList();

            return musicBrainzTagsList.Where(x => tagsIdsListForArtist.Contains(x.Id)).ToList();
        }
    }
}
