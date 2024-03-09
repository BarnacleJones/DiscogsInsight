using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.ViewModels.EntityViewModels;
using DiscogsInsight.ViewModels.Results;

namespace DiscogsInsight.View.Services.Artist
{
    public class ArtistViewService
    {
        private readonly ArtistDataService _artistDataService;
        public ArtistViewService(ArtistDataService artistDataService)
        {
            _artistDataService = artistDataService;
        }

        public async Task<ViewResult<List<ArtistViewModel>>> GetArtists()
        {
            try
            {
                var artists = await _artistDataService.GetArtists();

                var data = artists.Select(x => new ArtistViewModel
                {
                    Artist = x.Name,
                    DiscogsArtistId = x.DiscogsArtistId

                }).ToList();

                return new ViewResult<List<ArtistViewModel>>
                {
                    Data = data,
                    ErrorMessage = "",
                    Success = true
                };

            }
            catch (Exception ex)
            {
                return new ViewResult<List<ArtistViewModel>>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }

        public async Task<ViewResult<ArtistViewModel>> GetArtist(int? discogsArtistId)
        {
            try
            {
                var artist = await _artistDataService.GetArtist(discogsArtistId);

                var data = new ArtistViewModel
                {
                    Artist = artist.Name,
                    ArtistDescription = artist.Profile,
                    DiscogsArtistId = artist.DiscogsArtistId,
                    City = artist.City,
                    Country = artist.Country,  
                    StartYear = artist.StartYear,
                    EndYear = artist.EndYear,
                    MusicBrainzArtistId = artist.MusicBrainzArtistId
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
