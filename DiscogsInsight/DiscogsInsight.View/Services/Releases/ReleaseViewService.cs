using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.DataAccess.Entities;
using DiscogsInsight.ViewModels.EntityViewModels;
using DiscogsInsight.ViewModels.Results;
using DiscogsInsight.ViewModels.Collection;

namespace DiscogsInsight.View.Services.Releases
{
    public class ReleaseViewService
    {
        private readonly ReleaseDataService _releaseDataService;
        private readonly ArtistDataService _artistDataService;
        private readonly TracksDataService _tracksDataService;
        private readonly DiscogsGenresAndTagsDataService _discogsGenresAndTagsDataService;

        public ReleaseViewService(ReleaseDataService releaseDataService, ArtistDataService artistDataService, TracksDataService tracksDataService, DiscogsGenresAndTagsDataService discogsGenresAndTagsDataService)
        {
            _releaseDataService = releaseDataService;
            _artistDataService = artistDataService;
            _tracksDataService = tracksDataService;
            _discogsGenresAndTagsDataService = discogsGenresAndTagsDataService;
        }

        public async Task<bool> SetFavouriteBooleanOnRelease(bool favourited, int discogsReleaseId)
        {
            var success = await _releaseDataService.SetFavouriteBooleanOnRelease(favourited, discogsReleaseId);
            return success;
        }
                
        public async Task<ViewResult<ReleaseViewModel>> GetRelease(int? discogsReleaseId)
        {
            try
            {
                if (discogsReleaseId == null)
                    throw new Exception($"Missing release info");

                var release = await _releaseDataService.GetRelease(discogsReleaseId);

                if (release.Item1 == null)
                    throw new Exception("No release");

                var artist = await _artistDataService.GetArtist(release.Item1.DiscogsArtistId);

                var releaseTrackList = await _tracksDataService.GetTracksForRelease(release.Item1.DiscogsReleaseId);
                
                var data = await GetReleaseViewModel(release.Item1, releaseTrackList, artist.Name, release.Item2);

                return new ViewResult<ReleaseViewModel>
                {
                    Data = data,
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<ReleaseViewModel>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }

        public async Task<ViewResult<ReleaseViewModel>> GetRandomRelease()
        {
            try
            {
                var release =  await _releaseDataService.GetRandomRelease();

                var tracks = await _tracksDataService.GetTracksForRelease(release.Item1.DiscogsReleaseId);
                var artist = await _artistDataService.GetArtist(release.Item1.DiscogsArtistId, true);

                var data = await GetReleaseViewModel(release.Item1, tracks, artist.Name, release.Item2);

                return new ViewResult<ReleaseViewModel>
                {
                    Data = data,
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<ReleaseViewModel>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }

        public async Task<ViewResult<List<ReleaseViewModel>>> GetReleasesByGenreId(int genreId)
        {
            try
            {
                var returnedReleases = new List<ReleaseViewModel>();
                var allReleases = await _releaseDataService.GetAllReleasesAsList();
                var releaseGenreJoiningTable = await _discogsGenresAndTagsDataService.GetDiscogsGenreTagToDiscogsReleaseAsList();

                var releasesIdsWithThisGenre = releaseGenreJoiningTable.Where(x => x.DiscogsGenreTagId == genreId).Select(x => x.DiscogsReleaseId).ToList();
                var genreTag = await _discogsGenresAndTagsDataService.GetAllGenreTagsAsList();
                var thisSpecificGenre = genreTag.Where(x => x.Id == genreId).Select(x => x.DiscogsTag).FirstOrDefault();
                var releasesByGenre = allReleases.Where(x => releasesIdsWithThisGenre.Contains(x.DiscogsReleaseId)).ToList();

                foreach (var item in releasesByGenre)
                {
                    var thisItem = item;
                    var tracks = await _tracksDataService.GetTracksForRelease(item.DiscogsReleaseId);
                    var releaseTracks = tracks.Where(x => x.DiscogsReleaseId == thisItem.DiscogsReleaseId).ToList();
                          
                    var artist = await _artistDataService.GetArtist(item.DiscogsArtistId, true);
                    var image = await _releaseDataService.GetImageForRelease(item.MusicBrainzReleaseId);
                    returnedReleases.Add(await GetReleaseViewModel(item, releaseTracks, artist.Name, image));
                }

                return new ViewResult<List<ReleaseViewModel>>
                {
                    Data = returnedReleases,
                    ErrorMessage = thisSpecificGenre ?? "",//toDo: using this to get the genre name yes this is hacky, I will fix eventually
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<List<ReleaseViewModel>>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }
        
        public async Task<ViewResult<List<ReleaseViewModel>>> GetReleasesByYear(int? releaseYear)
        {
            try
            {
                var returnedReleases = new List<ReleaseViewModel>();
                var allReleases = await _releaseDataService.GetAllReleasesAsList();
                var releasesByYear = allReleases.Where(x => x.Year == releaseYear);

                foreach (var item in releasesByYear)
                {
                    var thisItem = item;
                    var tracks = await _tracksDataService.GetTracksForRelease(item.DiscogsReleaseId);
                    var releaseTracks = tracks.Where(x => x.DiscogsReleaseId == thisItem.DiscogsReleaseId).ToList();
                          
                    var artist = await _artistDataService.GetArtist(item.DiscogsArtistId, true);
                    var image = await _releaseDataService.GetImageForRelease(item.MusicBrainzReleaseId);
                    returnedReleases.Add(await GetReleaseViewModel(item, releaseTracks, artist.Name, image));
                }

                return new ViewResult<List<ReleaseViewModel>>
                {
                    Data = returnedReleases,
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<List<ReleaseViewModel>>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }
        
        public async Task<ViewResult<List<ReleaseViewModel>>> GetNewestReleases()
        {
            try
            {
                var returnedReleases = new List<ReleaseViewModel>();
                var newestNumber = 5; //todo: will update this to a setting value
                var newestFiveReleases = await _releaseDataService.GetNewestReleases(newestNumber);

                foreach (var item in newestFiveReleases)
                {
                    var thisItem = item;
                    var tracks = await _tracksDataService.GetTracksForRelease(item.DiscogsReleaseId);
                    var releaseTracks = tracks.Where(x => x.DiscogsReleaseId == thisItem.DiscogsReleaseId).ToList();
                          
                    var artist = await _artistDataService.GetArtist(item.DiscogsArtistId, true);
                    var image = await _releaseDataService.GetImageForRelease(item.MusicBrainzReleaseId);
                    returnedReleases.Add(await GetReleaseViewModel(item, releaseTracks, artist.Name, image));
                }

                return new ViewResult<List<ReleaseViewModel>>
                {
                    Data = returnedReleases,
                    ErrorMessage = "",
                    Success = true
                };

            }
            catch (Exception ex)
            {
                return new ViewResult<List<ReleaseViewModel>>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }

        private async Task<ReleaseViewModel> GetReleaseViewModel(Release release, List<Track> trackList, string? releaseArtistName, byte[]? imageAsBytes)
        {
            var trackListAsViewModel = trackList.Select(x => new TracksItemViewModel
            {
                Duration = x.MusicBrainzTrackLength == null
               ? x.Duration
               : TimeSpan.FromMilliseconds(x.MusicBrainzTrackLength.Value).ToString(@"mm\:ss"),
                Position = x.Position,
                Title = x.Title,
                Rating = x.Rating ?? 0,
                DiscogsArtistId = x.DiscogsArtistId ?? 0,
                DiscogsReleaseId = x.DiscogsReleaseId ?? 0
            }).ToList();

            var genres = await _discogsGenresAndTagsDataService.GetGenresForDiscogsRelease(release.DiscogsReleaseId);

            return new ReleaseViewModel
            {
                Artist = releaseArtistName ?? "Missing Artist",
                Year = release.Year.ToString(),
                OriginalReleaseYear = release.OriginalReleaseYear,
                ReleaseCountry = release.ReleaseCountry,
                Title = release.Title ?? "Missing Title",
                ReleaseNotes = release.ReleaseNotes ?? "",
                Genres = genres,
                DiscogsReleaseUrl = release.DiscogsReleaseUrl,
                Tracks = trackListAsViewModel,
                DateAdded = release.DateAdded,
                DiscogsArtistId = release.DiscogsArtistId,
                DiscogsReleaseId = release.DiscogsReleaseId,
                CoverImage = imageAsBytes, //null is ok there is a default image for null,
                IsFavourited = release.IsFavourited,
            };
        }

        public async Task<List<ReleaseViewModel>> GetAllReleaseViewModelsForArtistByDiscogsArtistId(int? discogsArtistId, string artistName)
        {
            var discogsReleaseIdsForArtist = await _releaseDataService.GetAllDiscogsReleaseIdsForArtist(discogsArtistId);
            var listToReturn = new List<ReleaseViewModel>();

            foreach (var releaseId in discogsReleaseIdsForArtist)
            {
                var release = await _releaseDataService.GetRelease(releaseId);

                var releaseTrackList = await _tracksDataService.GetTracksForRelease(release.Item1.DiscogsReleaseId);

                var data = await GetReleaseViewModel(release.Item1, releaseTrackList, artistName, release.Item2);

                listToReturn.Add(data);
            }

            return listToReturn;
        }
    }
}
