using DiscogsInsight.Database.Entities;

namespace DiscogsInsight.Tests.Common
{
    /// <summary>
    /// Generates dummy data for mocking database entities in various unit tests
    /// </summary>
    public static class DatabaseDataGenerator
    {
        #region Artist

        public static List<Artist> GetSampleArtists()
        {
            var artists = new List<Artist>();
            for (int i = 0; i < 6; i++)
            {
                var artist = new Artist()
                {
                    Id = i * 100,
                    DiscogsArtistId = i * 100,
                    Name = $"Artist {i}",
                    Profile = $"Profile {i}",
                    MusicBrainzArtistId = Guid.NewGuid().ToString(),
                    Country = "Country",
                    City = "City",
                    StartYear = "2000",
                    EndYear = "2022"
                };
                artists.Add(artist);
            }
            return artists;
        }

        public static List<Artist> GetSampleArtistsButOneNeedsDiscogsAndMusicBrainzData()
        {
            var artists = new List<Artist>();
            for (int i = 0; i < 6; i++)
            {
                var artist = new Artist()
                {
                    Id = i * 100,
                    DiscogsArtistId = i * 100,
                    Name = $"Artist {i}",
                    Profile = i != 5 ? $"Profile {i}" : null,
                    MusicBrainzArtistId = i != 5 ? Guid.NewGuid().ToString() : null,
                    Country = "Country",
                    City = "City",
                    StartYear = "2000",
                    EndYear = "2022"
                };
                artists.Add(artist);
            }
            return artists;
        }

        public static List<Artist> GetSampleArtistsButOneIsVarious()
        {
            var artists = new List<Artist>();
            for (int i = 0; i < 6; i++)
            {
                var artist = new Artist()
                {
                    Id = i * 100,
                    DiscogsArtistId = i * 100,
                    Name = i != 5 ? $"Artist {i}" : "Various",
                    Profile = i != 5 ? $"Profile {i}" : null,
                    MusicBrainzArtistId = Guid.NewGuid().ToString(),
                    Country = "Country",
                    City = "City",
                    StartYear = "2000",
                    EndYear = "2022"
                };
                artists.Add(artist);
            }
            return artists;
        }

        #endregion

        #region Track
        public static List<Track> GetUniqueSampleTracks()
        {
            var tracks = new List<Track>();
            for (int i = 0; i < 6; i++)
            {
                var a = new Track()
                {
                    Id = i,
                    DiscogsArtistId = i * 100,
                    DiscogsReleaseId = i * 100,
                    DiscogsMasterId = i * 100,
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

        #region Release

        public static List<Release> GetSampleReleases()
        {
            var releases = new List<Release>();

            for (int i = 1; i <= 5; i++)
            {
                var release = new Release
                {
                    Id = i,
                    DiscogsArtistId = i * 100,
                    DiscogsReleaseId = i * 100,
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

        #region MusicBrainzTags
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

        #region MusicBrainzArtistToMusicBrainzTags


        public static List<MusicBrainzArtistToMusicBrainzTags> GetSampleMusicBrainzArtistToMusicBrainzTags()
        {
            var musicBrainzArtistToMusicBrainzTags = new List<MusicBrainzArtistToMusicBrainzTags>();
            for (int i = 0; i < 6; i++)
            {
                var item = new MusicBrainzArtistToMusicBrainzTags()
                {
                    Id = i,
                    TagId = i,
                    MusicBrainzArtistId = $"MBID_{i}"
                };
                musicBrainzArtistToMusicBrainzTags.Add(item);
            }
            return musicBrainzArtistToMusicBrainzTags;
        }

        #endregion

        #region MusicBrainzArtistToMusicBrainzRelease


        public static List<MusicBrainzArtistToMusicBrainzRelease> GetSampleMusicBrainzArtistToMusicBrainzReleases()
        {
            var musicBrainzArtistToMusicBrainzReleases = new List<MusicBrainzArtistToMusicBrainzRelease>();
            for (int i = 0; i < 6; i++)
            {
                var item = new MusicBrainzArtistToMusicBrainzRelease()
                {
                    Id = i,
                    MusicBrainzArtistId = $"MBID_{i}",
                    MusicBrainzReleaseId = $"MBRID_{i}",
                    IsAReleaseGroupGroupId = i % 2 == 0,
                    MusicBrainzReleaseName = $"Release {i}",
                    ReleaseYear = "2000",
                    DiscogsArtistId = i,
                    Status = "official"
                };
                musicBrainzArtistToMusicBrainzReleases.Add(item);
            }
            return musicBrainzArtistToMusicBrainzReleases;
        }

        #endregion

        #region MusicBrainzReleaseToCoverImage

        public static List<MusicBrainzReleaseToCoverImage> GetSampleMusicBrainzReleaseToCoverImages()
        {
            var musicBrainzReleaseToCoverImages = new List<MusicBrainzReleaseToCoverImage>();
            for (int i = 0; i < 6; i++)
            {
                byte[] dummyImage = GenerateDummyImage();

                var item = new MusicBrainzReleaseToCoverImage()
                {
                    Id = i,
                    MusicBrainzReleaseId = $"mbid_{i * 100}",
                    MusicBrainzCoverImage = dummyImage
                };
                musicBrainzReleaseToCoverImages.Add(item);
            }
            return musicBrainzReleaseToCoverImages;
        }

        private static byte[] GenerateDummyImage()
        {
            Random rand = new Random();
            byte[] image = new byte[100];
            rand.NextBytes(image);
            return image;
        }


        #endregion

        #region DiscogsGenreTagToDiscogsRelease


        public static List<DiscogsGenreTagToDiscogsRelease> GetSampleDiscogsGenreTagToDiscogsReleases()
        {
            var discogsGenreTagToDiscogsReleases = new List<DiscogsGenreTagToDiscogsRelease>();
            for (int i = 0; i < 6; i++)
            {
                var item = new DiscogsGenreTagToDiscogsRelease()
                {
                    Id = i * 100,
                    DiscogsGenreTagId = i * 100,
                    DiscogsArtistId = i * 100,
                    DiscogsReleaseId = i * 100
                };
                discogsGenreTagToDiscogsReleases.Add(item);
            }
            return discogsGenreTagToDiscogsReleases;
        }
        #endregion

        #region DiscogsGenreTags


        public static List<DiscogsGenreTags> GetSampleDiscogsGenreTags()
        {
            var discogsGenreTags = new List<DiscogsGenreTags>();
            for (int i = 0; i < 6; i++)
            {
                var item = new DiscogsGenreTags()
                {
                    Id = i * 100,
                    DiscogsTag = $"Tag {i}"
                };
                discogsGenreTags.Add(item);
            }
            return discogsGenreTags;
        }

        #endregion
    }
}
