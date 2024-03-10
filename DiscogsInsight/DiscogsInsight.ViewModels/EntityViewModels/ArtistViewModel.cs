namespace DiscogsInsight.ViewModels.EntityViewModels
{
    public class ArtistViewModel
    {
        public string? Artist { get; set; }
        public string? ArtistDescription { get; set; }
        public int? DiscogsArtistId { get; set; }

        //todo: add releases by this artist - will need to be fetched using musicbrainz artist id for main artist call

        public string? MusicBrainzArtistId { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? StartYear { get; set; }
        public string? EndYear { get; set; }

        public List<string> Tags { get; set; }
    }
}
