using DiscogsInsight.Service.Models.EntityViewModels;
using DiscogsInsight.Service.Models.Results;
using DiscogsInsight.Service.Models.Collection;
using DiscogsInsight.Service.Models.DataCorrection;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Models;
namespace DiscogsInsight.Service.Releases
{
    public class ReleaseViewService
    {
        private readonly IReleaseDataService _releaseDataService;
        private readonly IArtistDataService _artistDataService;

        public ReleaseViewService(IReleaseDataService releaseDataService, IArtistDataService artistDataService)
        {
            _releaseDataService = releaseDataService;
            _artistDataService = artistDataService;
        }
        public async Task<ViewResult<ReleaseViewModel>> GetRelease(int? discogsReleaseId)
        {
            try
            {
                if (discogsReleaseId is null) throw new Exception("discogsReleaseId was null");

                var release = await _releaseDataService.GetReleaseDataModelByDiscogsReleaseId(discogsReleaseId.Value);
                if (release is null) throw new Exception("Release was null");
      
                return new ViewResult<ReleaseViewModel>
                {
                    Data = GetReleaseViewModel(release),
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
                var newestNumber = 5; //todo: will update this to a setting value
                var newestFiveReleases = await _releaseDataService.GetNewestReleases(newestNumber);
                var returnedReleases = newestFiveReleases.Select(x => GetReleaseViewModel(x)).ToList();

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
        public async Task<List<ReleaseViewModel>> GetAllReleaseViewModelsForArtistByDiscogsArtistId(int? discogsArtistId)
        {
            if (discogsArtistId == null) { throw new ArgumentNullException(nameof(discogsArtistId)); }
            var releaseData = await _releaseDataService.GetAllReleaseDataModelsForArtist(discogsArtistId.Value);

            var returnedReleases = new List<ReleaseViewModel>();

            foreach (var release in releaseData)
            {
                if (release is null) continue;
                returnedReleases.Add(GetReleaseViewModel(release));
            }
            return returnedReleases;
        }             
        public async Task<ViewResult<ReleaseViewModel>> GetRandomRelease()
        {
            try
            {
                var release = await _releaseDataService.GetRandomRelease();

                return new ViewResult<ReleaseViewModel>
                {
                    Data = GetReleaseViewModel(release),
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
                
                var releaseData = await _releaseDataService.GetReleaseDataModelsByDiscogsGenreTagId(discogsGenreTagId);

                var returnedReleases = new List<ReleaseViewModel>();
                foreach (var release in releaseData)
                {
                    if (release is null) continue;
                    returnedReleases.Add(GetReleaseViewModel(release));
                }

                return new ViewResult<List<ReleaseViewModel>>
                {
                    Data = returnedReleases,
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
                if (releaseYear == null) { throw new ArgumentNullException(nameof(releaseYear)); }
                var releaseDataList = await _releaseDataService.GetAllReleaseDataModelsByYear(releaseYear.Value);
                
                var listToReturn = releaseDataList.Select(x => GetReleaseViewModel(x)).ToList();

                return new ViewResult<List<ReleaseViewModel>>
                {
                    Data = listToReturn,
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
            await _releaseDataService.SetFavouriteBooleanOnRelease(favourited, discogsReleaseId);
            return true;
        }
        private ReleaseViewModel GetReleaseViewModel(ReleaseDataModel release)
        {
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
            return releaseViewModel;
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

    }
}
