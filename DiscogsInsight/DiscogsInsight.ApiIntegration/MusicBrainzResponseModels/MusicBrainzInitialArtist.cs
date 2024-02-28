using System.Text.Json.Serialization;

namespace DiscogsInsight.ApiIntegration.MusicBrainzResponseModels
{
    public class MusicBrainzInitialArtist
    {
        [JsonPropertyName("created")]
        public DateTime? Created { get; set; }

        [JsonPropertyName("count")]
        public int? Count { get; set; }

        [JsonPropertyName("offset")]
        public int? Offset { get; set; }

        [JsonPropertyName("release-groups")]
        public List<ReleaseGroup> ReleaseGroups { get; set; }
    }

    public class Alias
    {
        [JsonPropertyName("sort-name")]
        public string SortName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("primary")]
        public bool? Primary { get; set; }

        [JsonPropertyName("begin-date")]
        public string BeginDate { get; set; }

        [JsonPropertyName("end-date")]
        public string EndDate { get; set; }

        [JsonPropertyName("type-id")]
        public string TypeId { get; set; }
    }


    public class ReleaseGroup
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type-id")]
        public string TypeId { get; set; }

        [JsonPropertyName("score")]
        public int? Score { get; set; }

        [JsonPropertyName("primary-type-id")]
        public string PrimaryTypeId { get; set; }

        [JsonPropertyName("count")]
        public int? Count { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("first-release-date")]
        public string FirstReleaseDate { get; set; }

        [JsonPropertyName("primary-type")]
        public string PrimaryType { get; set; }

        [JsonPropertyName("secondary-types")]
        public List<string> SecondaryTypes { get; set; }

        [JsonPropertyName("secondary-type-ids")]
        public List<string> SecondaryTypeIds { get; set; }

        [JsonPropertyName("artist-credit")]
        public List<ArtistCredit> ArtistCredit { get; set; }

        [JsonPropertyName("releases")]
        public List<Release> Releases { get; set; }

        [JsonPropertyName("tags")]
        public List<Tag> Tags { get; set; }

        [JsonPropertyName("disambiguation")]
        public string Disambiguation { get; set; }
    }



}
