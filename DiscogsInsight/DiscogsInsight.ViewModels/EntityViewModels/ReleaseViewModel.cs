namespace DiscogsInsight.ViewModels.EntityViewModels
{
    public class ReleaseViewModel
    {
        public int? Year { get; set; }
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public string? ReleaseCountry { get; set; }
        public int? DiscogsArtistId { get; set; }
        public int? DiscogsReleaseId { get; set; }
        public string? Genres { get; set; }
        public DateTime? DateAdded { get; set; }

        public List<TrackViewModel>? Tracks { get; set; }

        public byte[] CoverImage { get; set; }
    }
}
