using System.Text.Json.Serialization;

namespace DiscogsInsight.ApiIntegration.Contract.MusicBrainzResponseModels
{
    public class MusicBrainzInitialArtist
    {
        [JsonPropertyName("created")]
        public DateTime? Created { get; set; }

        [JsonPropertyName("count")]
        public int? Count { get; set; }

        [JsonPropertyName("offset")]
        public int? Offset { get; set; }

        [JsonPropertyName("artists")]
        public List<Artist> Artists { get; set; }
    }


}
