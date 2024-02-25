using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.ViewModels.EntityViewModels;

namespace DiscogsInsight.View.Services.Artist
{
    public class ArtistViewService
    {
        private readonly ArtistDataService _artistDataService;
        public ArtistViewService(ArtistDataService artistDataService)
        {
            _artistDataService = artistDataService;
        }

        public async Task<List<ArtistViewModel>> GetArtists()
        {
            var artists = await _artistDataService.GetArtists();

            return artists.Select(x => new ArtistViewModel
            {
                Artist = x.Name,
                DiscogsArtistId = x.DiscogsArtistId

            }).ToList();
        }

        public async Task<ArtistViewModel> GetArtist(int? discogsArtistId)
        {
            var artist = await _artistDataService.GetArtist(discogsArtistId);

            return new ArtistViewModel
            {
                Artist = artist.Name,
                ArtistDescription = artist.Profile,
                DiscogsArtistId = artist.DiscogsArtistId
            };

        }
    }
}
