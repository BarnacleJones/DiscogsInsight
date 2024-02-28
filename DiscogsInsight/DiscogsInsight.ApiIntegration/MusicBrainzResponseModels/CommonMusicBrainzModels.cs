using System.Text.Json.Serialization;

namespace DiscogsInsight.ApiIntegration.MusicBrainzResponseModels
{
    public class Area
    {
        [JsonPropertyName("type-id")]
        public object TypeId { get; set; }

        [JsonPropertyName("disambiguation")]
        public string Disambiguation { get; set; }

        [JsonPropertyName("type")]
        public object Type { get; set; }

        [JsonPropertyName("sort-name")]
        public string SortName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("iso-3166-1-codes")]
        public List<string> Iso31661Codes { get; set; }
    }



    public class Artist
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("sort-name")]
        public string SortName { get; set; }

        [JsonPropertyName("disambiguation")]
        public string Disambiguation { get; set; }

        [JsonPropertyName("aliases")]
        public List<Alias> Aliases { get; set; }
    }

    public class TextRepresentation
    {
        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("script")]
        public string Script { get; set; }
    }


    public class ReleaseEvent
    {
        [JsonPropertyName("area")]
        public Area Area { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }
    }


    public class ArtistCredit
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("artist")]
        public Artist Artist { get; set; }

        [JsonPropertyName("joinphrase")]
        public string Joinphrase { get; set; }
    }


    //get a lot more populated on release once you have musicbrianz artist id
    public class Release
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("barcode")]
        public string Barcode { get; set; }

        [JsonPropertyName("aliases")]
        public List<object> Aliases { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("status-id")]
        public string StatusId { get; set; }

        [JsonPropertyName("disambiguation")]
        public string Disambiguation { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("release-events")]
        public List<ReleaseEvent> ReleaseEvents { get; set; }

        [JsonPropertyName("quality")]
        public string Quality { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("packaging")]
        public string Packaging { get; set; }

        [JsonPropertyName("text-representation")]
        public TextRepresentation TextRepresentation { get; set; }

        [JsonPropertyName("packaging-id")]
        public string PackagingId { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    public class Tag
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("count")]
        public int? Count { get; set; }
    }
}