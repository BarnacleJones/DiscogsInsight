namespace DiscogsInsight.DataAccess.Models
{
    public class ReleaseStatisticData
    {
        public int? Year { get; set; }
        public string OriginalReleaseYear { get; set; }
        public DateTime? DateAdded { get; set; }
        public string ReleaseCountry { get; set; }
        public string Title { get; set; }
    }
}
