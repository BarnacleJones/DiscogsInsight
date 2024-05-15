using DiscogsInsight.ApiIntegration.MusicBrainzResponseModels;
using DiscogsInsight.DataAccess.Entities;
using Track = DiscogsInsight.DataAccess.Entities.Track;

namespace DiscogsInsight.DataAccess.Tests
{
    public static class DummyDataGenerator
    {
        #region Tracks
        public static List<Track> GetUniqueSampleTracks()
        {
            var tracks = new List<Track>();
            for (int i = 0; i < 6; i++)
            {
                var a = new Track()
                {
                    Id = i,
                    DiscogsArtistId = i,
                    DiscogsReleaseId = i,
                    DiscogsMasterId = i,
                    Title = $"Track {i}",
                    Duration = "3:45",
                    MusicBrainzTrackLength = i,
                    Position = $"A{i}",
                    Rating = i
                };
                tracks.Add(a);
            }
            return tracks;

        }

        #endregion

        #region Albums/Releases

        public static List<Entities.Release> GetSampleReleases()
        {
            var releases = new List<Entities.Release>();

            for (int i = 1; i <= 5; i++)
            {
                var release = new Entities.Release
                {
                    Id = i,
                    DiscogsArtistId = i * 100,
                    DiscogsReleaseId = i,
                    DiscogsMasterId = i * 300,
                    Title = $"Release {i}",
                    DateAdded = DateTime.Now.AddDays(-i * 10),
                    Year = 1990 + i,
                    OriginalReleaseYear = (1990 + i).ToString(),
                    DiscogsReleaseUrl = $"https://www.discogs.com/release/{i * 200}",
                    ReleaseCountry = "US",
                    ReleaseNotes = $"Sample release notes for Release {i}",
                    MusicBrainzReleaseId = $"mbid-{i * 100}",
                    IsAReleaseGroupGroupId = i % 2 == 0,
                    MusicBrainzCoverUrl = $"https://coverartarchive.org/release/{i * 200}/front-500.jpg",
                    IsFavourited = i % 2 == 0,
                    HasAllApiData = true,
                    ArtistHasBeenManuallyCorrected = i % 3 == 0,
                    ReleaseHasBeenManuallyCorrected = i % 3 == 0
                };

                releases.Add(release);
            }

            return releases;
        }

        #endregion

        #region Artists

        //musicbrainz
        public static MusicBrainzInitialArtist GetSampleMusicBrainzInitialArtistResponse()
        {

            var artist = new MusicBrainzInitialArtist
            {
                Created = DateTime.Now.AddDays(-1 * 10),
                Count = 1,
                Offset = 0,
                Artists = new List<ApiIntegration.MusicBrainzResponseModels.Artist>()
            };

            for (int i= 0; i <= 3; i++)
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
                    Tags = new List<Tag> { new Tag { Count = i, Name = $"Tag{i}"} }
                };

                artist.Artists.Add(sampleArtist);
            }           

            return artist;
        }

        public static List<MusicBrainzTags> GetSampleMusicBrainzTags() 
        {
            var list = new List<MusicBrainzTags>();
            for (int i = 0; i < 5; i++)
            {
                var a = new MusicBrainzTags
                { 
                    Id = i, 
                    Tag = $"Tag{i}" 
                };
                list.Add(a);
            }
            return list;
        }

        #endregion
    }
}
