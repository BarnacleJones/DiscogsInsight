using DiscogsInsight.ApiIntegration.Contract;
using DiscogsInsight.ApiIntegration.Models.MusicBrainzResponseModels;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.Database.Contract;
using DiscogsInsight.Database.Entities;
using System.Formats.Asn1;
using Artist = DiscogsInsight.Database.Entities.Artist;

namespace DiscogsInsight.DataAccess.Services
{
    public class ArtistDataService : IArtistDataService
    {
        private readonly IDiscogsApiService _discogsApiService;
        private readonly ISQLiteAsyncConnection _db;
        private readonly IMusicBrainzApiService _musicBrainzApiService;

        public ArtistDataService(IDiscogsApiService discogsApiService,
            ISQLiteAsyncConnection db,
            IMusicBrainzApiService musicBrainzApiService)
        {
            _db = db;
            _discogsApiService = discogsApiService;
            _musicBrainzApiService = musicBrainzApiService;
        }

       

        public async Task<int?> GetARandomDiscogsArtistId()
        {
            var randomArtistIdQuery = @$"SELECT DiscogsArtistId FROM Artist ORDER BY RANDOM() LIMIT 1;";
            var artistId = await _db.QueryAsync<DiscogsArtistIdClass>(randomArtistIdQuery);

            return artistId.FirstOrDefault().DiscogsArtistId;
        }

       

        public async Task<Artist?> GetArtistByDiscogsId(int? discogsArtistId, bool fetchAndSaveApiData = true)
        {
            if (discogsArtistId == null) { return new Artist { Name = "No Artist Id Supplied" }; }
            
            var existingArtist = await GetArtistByDiscogsIdFromDb(discogsArtistId.Value);

            if (existingArtist == null)
            {
                //dont want to store the artist if not in db already- I dont know how would one get here
                throw new Exception($"Unhandled exception: Artist {discogsArtistId} not in db. It might be a 'various artist' issue, or refresh your database");
            }
            if (existingArtist.Name?.ToLower() == "various")
            {
                //dont fetch api data - 'Various' 404s for discogs, and causes bad data with MusicBrainz. 
                return existingArtist;
            }
            if (fetchAndSaveApiData && !existingArtist.HasAllApiData)
            {
                if (string.IsNullOrWhiteSpace(existingArtist.Profile))
                {
                    var discogsResult = await _discogsApiService.GetArtistFromDiscogs(discogsArtistId.Value);
                    //Add additional properties wanted from Artist Discogs call here...profile is really the only useful one here
                    //If there is no profile, this api call will be made every time. But for now that is ok I guess.
                    existingArtist.Profile = discogsResult.profile;
                }
                if (existingArtist.MusicBrainzArtistId == null && existingArtist.Name != null)
                {
                    var musicBrainzResult = await _musicBrainzApiService.GetInitialArtistFromMusicBrainzApi(existingArtist.Name);
                    await SetMusicBrainzArtistDataForSavingAndSaveTagsFromArtistResponse(musicBrainzResult, existingArtist);
                }

                existingArtist.HasAllApiData = true;
                await _db.UpdateAsync(existingArtist);
            }

            return existingArtist;
            //used down on artist view service
            //it was then doing this after this call:


            //__________________________________________________________________________________________________________________________
            //var artist = await _artistDataService.GetArtistByDiscogsId(discogsArtistId);

            //var tags = await _tagsDataService.GetTagsByMusicBrainzArtistId(artist.MusicBrainzArtistId);
            //TagsDataService moved from here only used here - will get fixed with rewrite

            //public async Task<List<MusicBrainzTags>> GetTagsByMusicBrainzArtistId(string musicBrainzArtistId)
            //{
            //    var musicBrainzTagsList = await _db.Table<MusicBrainzTags>().ToListAsync();
            //    var musicBrainzTagsToArtistsTable = await _db.Table<MusicBrainzArtistToMusicBrainzTags>().ToListAsync();

            //    var musicBrainzTagsToArtistsList = musicBrainzTagsToArtistsTable.Where(x => x.MusicBrainzArtistId == musicBrainzArtistId);
            //    var tagsIdsListForArtist = musicBrainzTagsToArtistsList.Select(x => x.TagId).ToList();

            //    return musicBrainzTagsList.Where(x => tagsIdsListForArtist.Contains(x.Id)).ToList();
            //}

            //var tagsList = tags.Select(x => x.Tag).ToList();

            //var releasesByThisArtist = await _artistDataService.GetArtistsReleasesByMusicBrainzArtistId(artist.MusicBrainzArtistId);

            //__________________________________________________________________________________________________________________________
        }

        private async Task<Artist?> GetArtistByDiscogsIdFromDb(int discogsArtistId)
        {
            var discogsArtistIdsQuery = @$"SELECT * FROM Artist WHERE DiscogsArtistId = ?";

            var discogsArtistIdsInterim = await _db.QueryAsync<Artist>(discogsArtistIdsQuery);

            var artist = discogsArtistIdsInterim.FirstOrDefault();
            if (artist == null) return null;

            return artist;
        }

        #region still to rewrite


        private async Task SetMusicBrainzArtistDataForSavingAndSaveTagsFromArtistResponse(MusicBrainzInitialArtist artistResponse, Artist existingArtist)
        {               
            //**this makes an assumption that can cause bad data**
            //Artists in the response is a list, there are similar named artists in the list
            //It looks like the first in the list is closest match
            //Ability to correct this data is on the release card

            var musicBrainsArtistId = artistResponse.Artists.Select(x => x.Id).FirstOrDefault();

            existingArtist.MusicBrainzArtistId = musicBrainsArtistId;

            var artistArea = artistResponse.Artists
                                                    .Where(x => x.Id == musicBrainsArtistId)
                                                    .Select(x => x.Area)
                                                    .FirstOrDefault();
            var artistBeginArea = artistResponse.Artists
                                                    .Where(x => x.Id == musicBrainsArtistId)
                                                    .Select(x => x.BeginArea)
                                                    .FirstOrDefault();
                    
            if (artistBeginArea != null)
            {
                var isACity = artistBeginArea.Type == "City";
                var isACountry = artistBeginArea.Type == "Country";
                if (isACity)
                {
                    existingArtist.City = artistBeginArea.Name;
                }
                if (isACountry)
                {
                    existingArtist.Country = artistBeginArea.Name;
                }
            }
                    
            if (artistArea != null)
            {
                var isACity = artistArea.Type == "City";
                var isACountry = artistArea.Type == "Country";
                if (isACountry)
                {
                    existingArtist.Country = artistArea.Name;
                }
                if (isACity)
                {
                    existingArtist.City = artistArea.Name;
                }
            }

            existingArtist.StartYear = artistResponse.Artists
                                                    .Where(x => x.Id == musicBrainsArtistId)
                                                    .Select(x => x.LifeSpan?.Begin)
                                                    .FirstOrDefault();

            existingArtist.EndYear = artistResponse.Artists
                                                    .Where(x => x.Id == musicBrainsArtistId)
                                                    .Select(x => x.LifeSpan?.End)
                                                    .FirstOrDefault();

            //save tags - yes it is saving to joining table too before saving the actual artist table information, but its saving writes to the db....
            var success = await SaveTagsByMusicBrainzArtistId(artistResponse, musicBrainsArtistId ?? "");
        }
        private async Task SaveTagsByMusicBrainzArtistId(MusicBrainzInitialArtist artistResponse, string musicBrainzArtistId)
        {

            var artistFromResponse = artistResponse.Artists.Where(x => x.Id == musicBrainzArtistId).FirstOrDefault();
            if (artistFromResponse == null) return; //artist id mismatch potentially 

            var tagsInResponse = artistFromResponse.Tags == null ? new List<Tag>() : artistFromResponse.Tags.Where(x => x.Count >= 1).ToList();

            if (tagsInResponse != null && tagsInResponse.Any())
            {

                //query to go to get just tag names from musicbrainztags
                //make it a where not IN statement
                //var tagsTable = await _db.Table<MusicBrainzTags>().ToListAsync();
                // existingTagNamesInDb = tagsTable.Count != 0 ? tagsTable.Select(x => x.Tag).ToList() : new List<string?>();
                
                var data = string.Join(",", tagsInResponse);//need to make this add single commas dudoy

                var releasesByReleaseIdsQuery = @$"
                            SELECT 
                            Tag
                            FROM MusicBrainzTags
                            WHERE NOT TAG IN ({data});";

                var tagsNotInList = await _db.QueryAsync<TagNameQueryClass>(releasesByReleaseIdsQuery);

                var tagsNotInDatabase = new List<string>();
                var tagsToSave = new List<MusicBrainzTags>();

                foreach (var tag in tagsInResponse)
                {
                    if (!tagsNotInDatabase.Contains(tag.Name))
                    {
                        var tagToSave = new MusicBrainzTags { Tag = tag.Name };
                        tagsToSave.Add(tagToSave);
                    }
                }

                await _db.InsertAllAsync(tagsToSave);
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

        public async Task<List<MusicBrainzArtistRelease>> GetArtistsReleasesByMusicBrainzArtistId(string musicBrainzArtistId)
        {
           
                var allReleasesTable = await _db.Table<MusicBrainzArtistToMusicBrainzRelease>().ToListAsync();

                var allReleasesByArtist = allReleasesTable.Where(x => x.MusicBrainzArtistId == musicBrainzArtistId).ToList();

                return allReleasesByArtist.Select(x => new MusicBrainzArtistRelease
                {
                    MusicBrainzReleaseName = x.MusicBrainzReleaseName ??  " " ,
                    ReleaseYear = x.ReleaseYear ?? " ",
                    Status = x.Status ?? " ",
                }).ToList();

         
        }


        #endregion

       
    }

}
