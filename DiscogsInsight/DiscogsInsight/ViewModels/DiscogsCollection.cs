namespace DiscogsInsight.ViewModels
{
    public class DiscogsCollection

    {
        public List<ReleaseViewModel> Releases { get; set; }

        public IEnumerable<string> ArtistList { get; set; }
    }
}
