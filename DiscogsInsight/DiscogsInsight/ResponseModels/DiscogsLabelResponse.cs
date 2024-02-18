namespace DiscogsInsight.ResponseModels
{
    public class DiscogsLabelResponse
    {
        public string profile { get; set; }
        public string releases_url { get; set; }
        public string name { get; set; }
        public string contact_info { get; set; }
        public string uri { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public List<ResponseSublabel> sublabels { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public List<string> urls { get; set; }
        public List<Image> images { get; set; }
        public string resource_url { get; set; }
        public int id { get; set; }
        public string data_quality { get; set; }
    }
}
