using DiscogsInsight.ApiIntegration.DiscogsResponseModels;
using DiscogsInsight.ApiIntegration.MusicBrainzResponseModels;
using DiscogsInsight.ApiIntegration.Services;
using DiscogsInsight.DataAccess.Entities;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.DataAccess.Services
{
    public class TagsDataService
    {
        private readonly DiscogsInsightDb _db;
        private readonly ILogger<TagsDataService> _logger;

        public TagsDataService(DiscogsInsightDb db, ILogger<TagsDataService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<bool> SaveTagsByMusicBrainzArtistId(MusicBrainzInitialArtist artistResponse, string musicBrainzArtistId)
        {
            try
            {
                var tagsTable = await _db.GetTable<MusicBrainzTags>();
                var listFromTagsTable = await tagsTable.ToListAsync();
                var existingTagNamesInDb = listFromTagsTable.Any() ? listFromTagsTable.Select(x => x.Tag).ToList() : new List<string?>();

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
                            await _db.SaveItemAsync(tagToSave);
                        }
                    }                
                }

                //get table again with newly saved tags to save to joining table
                tagsTable = await _db.GetTable<MusicBrainzTags>();
                var tagsTableList = await tagsTable.ToListAsync();
                
                foreach (var tag in tagsInResponse)
                {
                    var tagId = tagsTableList.Where(x => x.Tag == tag.Name).First().Id;
                    var tagToArtist = new MusicBrainzArtistToMusicBrainzTags { TagId = tagId, MusicBrainzArtistId = musicBrainzArtistId };
                    await _db.SaveItemAsync(tagToArtist);
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
            var musicBrainzTagsTable = await _db.GetTable<MusicBrainzTags>();
            var musicBrainzTagsToArtistsTable = await _db.GetTable<MusicBrainzArtistToMusicBrainzTags>();

            var musicBrainzTagsToArtistsList = await musicBrainzTagsToArtistsTable.Where(x => x.MusicBrainzArtistId == musicBrainzArtistId)            
                                                                               .ToListAsync();
            var musicBrainzTagsList = await musicBrainzTagsTable.ToListAsync();
            var tagsIdsListForArtist =  musicBrainzTagsToArtistsList.Select(x => x.TagId).ToList();

            return musicBrainzTagsList.Where(x => tagsIdsListForArtist.Contains(x.Id)).ToList();
        }
    }
}
