using DiscogsInsight.ApiIntegration.Models.DiscogsResponseModels;
using DiscogsInsight.ApiIntegration.Models.MusicBrainzResponseModels;

namespace DiscogsInsight.Tests.Common
{
    public static class DummyApiDataGenerator
    {
        /// <summary>
        /// Generates dummy data for mocking api responses in various unit tests
        /// </summary>
        public static MusicBrainzInitialArtist GetSampleMusicBrainzInitialArtistResponse()
        {

            var artist = new MusicBrainzInitialArtist
            {
                Created = DateTime.Now.AddDays(-1 * 10),
                Count = 1,
                Offset = 0,
                Artists = new List<Artist>()
            };

            for (int i = 0; i <= 3; i++)
            {
                var sampleArtist = new Artist
                {
                    Id = $"{i}",
                    Type = "Person",
                    TypeId = "person",
                    Score = 90,
                    Name = $"Artist {i}",
                    SortName = $"Artist{i}",
                    Country = "US",
                    LifeSpan = new LifeSpan { Begin = "here", End = "there" },
                    Area = new Area { Type = "City", Name = "Tellyland" },
                    Tags = new List<Tag> { new Tag { Count = i, Name = $"Tag{i}" } }
                };

                artist.Artists.Add(sampleArtist);
            }

            return artist;
        }
        public static DiscogsReleaseResponse GetSampleDiscogsReleaseResponse()
        {
            var release = new DiscogsReleaseResponse
            {
                title = "Sample Release",
                id = 1,
                artists = new List<ResponseArtist>(),
                data_quality = "High",
                thumb = "http://example.com/thumb.jpg",
                community = new ResponseCommunity
                {
                    contributors = new List<ResponseContributor>(),
                    data_quality = "High",
                    have = 100,
                    rating = new ResponseRating
                    {
                        average = 4.5,
                        count = 50
                    },
                    status = "Accepted",
                    submitter = new ResponseSubmitter
                    {
                        resource_url = "http://example.com/submitter",
                        username = "sample_user"
                    },
                    want = 50
                },
                companies = new List<ResponseCompany>(),
                country = "US",
                date_added = DateTime.Now.AddDays(-10),
                date_changed = DateTime.Now.AddDays(-5),
                estimated_weight = 300,
                extraartists = new List<ResponseExtraartist>(),
                format_quantity = 1,
                formats = new List<ResponseFormat>(),
                genres = new List<string> { "Rock", "Pop" },
                identifiers = new List<ResponseIdentifier>(),
                lowest_price = 10.99,
                master_id = 987654321,
                master_url = "http://example.com/master",
                notes = "Sample notes",
                num_for_sale = 20,
                released = "2024",
                released_formatted = "2024-06-03",
                resource_url = "http://example.com/release",
                series = new List<object>(),
                status = "Accepted",
                styles = new List<string> { "Alternative", "Indie" },
                tracklist = new List<ResponseTracklist>(),
                uri = "http://example.com/release",
                videos = new List<ResponseVideo>(),
                year = 2024
            };

            for (int i = 0; i <= 3; i++)
            {
                var sampleArtist = new ResponseArtist
                {
                    anv = $"Sample Anv {i}",
                    id = i,
                    join = "Main",
                    name = $"Artist {i}",
                    resource_url = $"http://example.com/artist/{i}",
                    role = "Composer",
                    tracks = "A1"
                };

                release.artists.Add(sampleArtist);
            }

            return release;
        }

        public static DiscogsArtistResponse GetSampleDiscogsArtistResponse(int discogsArtistIdToMatch)
        {
            return new DiscogsArtistResponse
            {
                id = discogsArtistIdToMatch,
                namevariations = new List<string> { "a", "b" },
                profile = "Fantastic",
                releases_url = "asdf",
                resource_url = "asdf",
                uri = "asdf",
                urls = new List<string> { "asdf" },
                data_quality = "asdf",
            };
        }
    }


}
