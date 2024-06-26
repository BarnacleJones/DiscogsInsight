namespace DiscogsInsight.DataAccess.Models
{
    public class CollectionStatisticData
    {
        public int ReleaseCount { get; set; }
        public int TracksCount { get; set; }
        public List<CollectionReleaseData> Releases { get; set; }
    }

    public class CollectionReleaseData 
    {
        public bool HasAllApiData { get; set; }
        public DateTime? DateAdded { get; set; }
    }
}
