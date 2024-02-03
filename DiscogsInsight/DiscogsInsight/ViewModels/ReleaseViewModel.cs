namespace DiscogsInsight.ViewModels
{
    public class ReleaseViewModel
    {
        public int Year { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string ResourceUrl { get; set; }
        public string MasterUrl { get; set; }
        public string Genres { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
