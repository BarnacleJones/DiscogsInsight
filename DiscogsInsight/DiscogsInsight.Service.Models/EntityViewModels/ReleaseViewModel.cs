using DiscogsInsight.Service.Models.Collection;

namespace DiscogsInsight.Service.Models.EntityViewModels
{
    public class ReleaseViewModel
    {
        public string? Year { get; set; }
        public string? OriginalReleaseYear { get; set; }
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public string? ReleaseNotes { get; set; }
        public string? ReleaseCountry { get; set; }
        public int? DiscogsArtistId { get; set; }
        public int? DiscogsReleaseId { get; set; }
        public List<ReleaseGenres>? Genres { get; set; }
        public string? DiscogsReleaseUrl { get; set; }
        public DateTime? DateAdded { get; set; }
        public List<TracksItemViewModel>? Tracks { get; set; }
        public byte[] CoverImage { get; set; }
        public bool IsFavourited { get; set; }
    }
    public class ReleaseGenres
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
