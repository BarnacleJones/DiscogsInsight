using DiscogsInsight.ApiIntegration.Contract.MusicBrainzResponseModels;
using DiscogsInsight.ApiIntegration.MusicBrainzResponseModels;

namespace DiscogsInsight.DataAccess.Tests
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
                Artists = new List<ApiIntegration.Contract.MusicBrainzResponseModels.Artist>()
            };

            for (int i = 0; i <= 3; i++)
            {
                var sampleArtist = new ApiIntegration.MusicBrainzResponseModels.Artist
                {
                    Id = $"{i}",
                    Type = "Person",
                    TypeId = "person",
                    Score = 90,
                    Name = $"Artist {i}",
                    SortName = $"Artist{i}",
                    Country = "US",
                    Tags = new List<Tag> { new Tag { Count = i, Name = $"Tag{i}" } }
                };

                artist.Artists.Add(sampleArtist);
            }

            return artist;
        }
    }
}
