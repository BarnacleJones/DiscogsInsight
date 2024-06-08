using DiscogsInsight.Database.Entities;
using DiscogsInsight.Service.Models.EntityViewModels;
using DiscogsInsight.Service.Models.Results;
using DiscogsInsight.Service.Models.Collection;
using DiscogsInsight.Service.Models.DataCorrection;
using DiscogsInsight.DataAccess.Contract;

namespace DiscogsInsight.Service.Releases
{
    public class ReleaseViewService
    {
        private readonly IReleaseDataService _releaseDataService;
        private readonly IArtistDataService _artistDataService;
        private readonly ITracksDataService _tracksDataService;
        private readonly IDiscogsGenresAndTagsDataService _discogsGenresAndTagsDataService;

        public ReleaseViewService(IReleaseDataService releaseDataService, IArtistDataService artistDataService, ITracksDataService tracksDataService, IDiscogsGenresAndTagsDataService discogsGenresAndTagsDataService)
        {
            _releaseDataService = releaseDataService;
            _artistDataService = artistDataService;
            _tracksDataService = tracksDataService;
            _discogsGenresAndTagsDataService = discogsGenresAndTagsDataService;
        }
        public async Task<ViewResult<ReleaseViewModel>> GetRelease(int? discogsReleaseId)
        {
            try
            {
                if (discogsReleaseId == null)
                    throw new Exception($"Missing release info");

                var release = await _releaseDataService.GetReleaseAndImageAndRetrieveAllApiDataForRelease(discogsReleaseId);

                if (release.Item1 == null)
                    throw new Exception("No release");

                var artist = await _artistDataService.GetArtistByDiscogsId(release.Item1.DiscogsArtistId, false);

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

                    var artist = await _artistDataService.GetArtistByDiscogsId(item.DiscogsArtistId, true);
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
        public async Task<ViewResult<ReleaseViewModel>> GetRandomRelease()
        {
            try
            {
                var release = await _releaseDataService.GetRandomRelease();

                var tracks = await _tracksDataService.GetTracksForRelease(release.Item1.DiscogsReleaseId);
                var artist = await _artistDataService.GetArtistByDiscogsId(release.Item1.DiscogsArtistId, true);

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

        public async Task<ViewResult<List<ReleaseViewModel>>> GetReleasesByDiscogsGenreTagId(int discogsGenreTagId)
        {
            try
            {
                var returnedReleases = new List<ReleaseViewModel>();
                
                var releaseData = await _releaseDataService.GetReleaseDataModelsByDiscogsGenreTagId(discogsGenreTagId);

                foreach (var release in releaseData)
                {
                    if (release is null) continue;

                    var releaseViewModel = new ReleaseViewModel
                    {
                        ReleaseCountry = release.ReleaseCountry,
                        Artist = release.Artist,
                        CoverImage = release.CoverImage,

                        DateAdded = release.DateAdded,
                        DiscogsArtistId = release.DiscogsArtistId,
                        DiscogsReleaseId = release.DiscogsReleaseId,
                        DiscogsReleaseUrl = release.DiscogsReleaseUrl,
                        Genres = release.Genres,
                        IsFavourited = release.IsFavourited,
                        OriginalReleaseYear = release.OriginalReleaseYear,
                        ReleaseNotes = release.ReleaseNotes,
                        Title = release.Title,
                        Year = release.Year
                    };

                    releaseViewModel.Tracks = new List<TracksItemViewModel>();

                    foreach (var trackDto in release.Tracks)
                    {
                        releaseViewModel.Tracks.Add(new TracksItemViewModel
                        {
                            Artist = trackDto.Artist,
                            DiscogsArtistId = trackDto.DiscogsArtistId,
                            DiscogsReleaseId = trackDto.DiscogsReleaseId,
                            Duration = trackDto.Duration,
                            Position = trackDto.Position,
                            Rating = trackDto.Rating,
                            Release = trackDto.Release,
                            Title = trackDto.Title
                        });
                    }

                    returnedReleases.Add(releaseViewModel);
                }

                return new ViewResult<List<ReleaseViewModel>>
                {
                    Data = returnedReleases,
                    //ErrorMessage = thisSpecificGenre ?? "",//toDo: using this to get the genre name yes this is hacky, I will fix eventually
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

                    var artist = await _artistDataService.GetArtistByDiscogsId(item.DiscogsArtistId, true);
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
        public async Task<bool> SetFavouriteBooleanOnRelease(bool favourited, int discogsReleaseId)
        {
            var success = await _releaseDataService.SetFavouriteBooleanOnRelease(favourited, discogsReleaseId);
            return success;
        }

        #region DataCorrection

        public async Task<ViewResult<List<CorrectArtistDataViewModel>>> GetPossibleArtistsBasedOnDiscogsReleaseId(int? discogsReleaseId)
        {
            try
            {

                var data = await _artistDataService.GetPossibleArtistsForDataCorrectionFromDiscogsReleaseId(discogsReleaseId);

                var viewModel = data.Select(x => new CorrectArtistDataViewModel
                {
                    ArtistName = x.ArtistName,
                    CorrectArtistMusicBrainzId = x.CorrectArtistMusicBrainzId,
                    Country = x.Country,
                    Disambiguation = x.Disambiguation,
                }).ToList();

                return new ViewResult<List<CorrectArtistDataViewModel>>
                {
                    Data = viewModel,
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<List<CorrectArtistDataViewModel>>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }
        public async Task<bool> UpdateArtistWithCorrectMusicBrainzId(int? discogsReleaseId, string musicBrainzId)
        {
            var success = await _artistDataService.DeleteExistingArtistDataAndUpdateToChosenMusicBrainzArtistFromMusicBrainzId(discogsReleaseId, musicBrainzId);
            return success;
        }
        public async Task<ViewResult<List<CorrectReleaseDataViewModel>>> GetPossibleReleasesBasedOnDiscogsReleaseId(int? discogsReleaseId)
        {
            try
            {
                var data = await _releaseDataService.GetPossibleReleasesForDataCorrectionFromDiscogsReleaseId(discogsReleaseId);

                var viewModel = data.Select(x => new CorrectReleaseDataViewModel
                {
                    Date = x.Date,
                    MusicBrainzReleaseId = x.MusicBrainzReleaseId,
                    Status = x.Status,
                    Title = x.Title
                }).ToList();

                return new ViewResult<List<CorrectReleaseDataViewModel>>
                {
                    Data = viewModel,
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<List<CorrectReleaseDataViewModel>>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }

        public async Task<bool> UpdateReleaseWithCorrectMusicBrainzId(int? discogsReleaseId, string musicBrainzReleaseId)
        {
            var success = await _releaseDataService.UpdateReleaseToBeNewMusicBrainzReleaseId(discogsReleaseId, musicBrainzReleaseId);
            return success;
        }
        #endregion

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
                CoverImage = imageAsBytes ?? [],
                IsFavourited = release.IsFavourited,
            };
        }

        public async Task<List<ReleaseViewModel>> GetAllReleaseViewModelsForArtistByDiscogsArtistId(int? discogsArtistId, string artistName)
        {
            var discogsReleaseIdsForArtist = await _releaseDataService.GetAllDiscogsReleaseIdsForArtist(discogsArtistId);
            var listToReturn = new List<ReleaseViewModel>();

            foreach (var releaseId in discogsReleaseIdsForArtist)
            {
                var release = await _releaseDataService.GetReleaseAndImageAndRetrieveAllApiDataForRelease(releaseId);

                var releaseTrackList = await _tracksDataService.GetTracksForRelease(release.Item1.DiscogsReleaseId);

                var data = await GetReleaseViewModel(release.Item1, releaseTrackList, artistName, release.Item2);

                listToReturn.Add(data);
            }

            return listToReturn;
        }
    }
}
