using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.Service.Models.EntityViewModels;
using DiscogsInsight.Service.Models.Results;

namespace DiscogsInsight.Service.Artist
{
    public class ArtistViewService
    {
        private readonly IArtistDataService _artistDataService;
        public ArtistViewService(IArtistDataService artistDataService)
        {
            _artistDataService = artistDataService;
        }

        public async Task<ViewResult<ArtistViewModel>> GetRandomArtist()
        {
            var allArtists = await _artistDataService.GetAllArtistsFromDatabase();

            var randomArtistId = allArtists.Select(x => x.DiscogsArtistId).OrderBy(r => Guid.NewGuid()).FirstOrDefault();//new GUID as key, will be random

            return await GetArtist(randomArtistId);
        }

        public async Task<ViewResult<ArtistViewModel>> GetArtist(int? discogsArtistId)
        {
            try
            {
                var artist = await _artistDataService.GetArtistByDiscogsId(discogsArtistId);

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

                var tagsList = tags.Select(x => x.Tag).ToList();

                var releasesByThisArtist = await _artistDataService.GetArtistsReleasesByMusicBrainzArtistId(artist.MusicBrainzArtistId);

                var releasesViewModel = releasesByThisArtist.GroupBy(x => x.Status)
                    .Select(x => new MusicBrainzArtistsReleasesViewModel
                    {
                        Status = x.Key,
                        Releases = x.OrderBy(y => y.ReleaseYear)
                                    .ThenBy(y => y.MusicBrainzReleaseName)
                                    .Select(y => new MusicBrainzReleaseViewModel
                                    {
                                        Title = y.MusicBrainzReleaseName,
                                        Year = y.ReleaseYear
                                    }).ToList()
                    }).ToList();


                
                
                //old line was calling to release view service
                //var releasesInCollection = await _releaseViewService.GetAllReleaseViewModelsForArtistByDiscogsArtistId(artist.DiscogsArtistId);

                //below
                //public async Task<List<ReleaseViewModel>> GetAllReleaseViewModelsForArtistByDiscogsArtistId(int? discogsArtistId)
                //{
                //    if (discogsArtistId == null) { throw new ArgumentNullException(nameof(discogsArtistId)); }
                //    var releaseData = await _releaseDataService.GetAllReleaseDataModelsForArtist(discogsArtistId.Value);

                //    var returnedReleases = new List<ReleaseViewModel>();

                //    foreach (var release in releaseData)
                //    {
                //        if (release is null) continue;
                //        returnedReleases.Add(GetReleaseViewModel(release));
                //    }
                //    return returnedReleases;
                //}

                var data = new ArtistViewModel
                {
                    Artist = artist.Name,
                    ArtistDescription = artist.Profile,
                    DiscogsArtistId = artist.DiscogsArtistId,
                    City = artist.City,
                    Country = artist.Country,
                    StartYear = artist.StartYear,
                    EndYear = artist.EndYear,
                    MusicBrainzArtistId = artist.MusicBrainzArtistId,
                    Tags = tagsList,
                    ArtistsReleases = releasesViewModel,
                    ReleasesInCollection = releasesInCollection
                };

                return new ViewResult<ArtistViewModel>
                {
                    Data = data,
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<ArtistViewModel>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }
    }
}
