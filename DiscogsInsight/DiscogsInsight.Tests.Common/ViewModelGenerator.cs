using DiscogsInsight.Service.Models.Collection;
using DiscogsInsight.Service.Models.EntityViewModels;

namespace DiscogsInsight.Tests.Common
{
    public static class ViewModelGenerator
    {
        /// <summary>
        /// Three items in list, two are of same artist/genre/tag
        /// </summary>
        /// <returns></returns>
        public static List<ReleaseViewModel> GetSampleReleaseViewModelList() 
        {
            var release1 = new ReleaseViewModel
            {
                Year = "2019",
                OriginalReleaseYear = "2019",
                Title = "Crystal Gazer",
                Artist = "Karkara",
                ReleaseNotes = "Exceltante",
                ReleaseCountry = "France",
                DiscogsArtistId = 1,
                DiscogsReleaseId = 1,
                Genres = new List<(string? Name, int Id)>()
                {
                    new("Psych", 1),
                    new("Rock", 2)
                },
                DiscogsReleaseUrl = "www.psych.com",
                Tracks = new List<TracksItemViewModel>
                {
                    new TracksItemViewModel
                    {
                        Title = "Proxima Centaury",
                        Position = "A1",
                        Rating = 5,
                        Artist = "Karkara",
                        Release = "Crystal Gazer",
                        DiscogsArtistId=1,
                        DiscogsReleaseId=1,
                    },
                    new TracksItemViewModel
                    {
                        Title = "Camel Rider",
                        Position = "A2",
                        Rating = 5,
                        Artist = "Karkara",
                        Release = "Crystal Gazer",
                        DiscogsArtistId=1,
                        DiscogsReleaseId=1,
                    },
                },
                CoverImage = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                IsFavourited = true
            };

            var release2 = new ReleaseViewModel
            {
                Year = "2024",
                OriginalReleaseYear = "2024",
                Title = "All Is Dust",
                Artist = "Karkara",
                ReleaseNotes = "Exceltante",
                ReleaseCountry = "France",
                DiscogsArtistId = 1,
                DiscogsReleaseId = 2,
                Genres = new List<(string? Name, int Id)>()
                {
                    new("Psych", 1),
                    new("Rock", 2)
                },
                DiscogsReleaseUrl = "www.psych.com",
                Tracks = new List<TracksItemViewModel>
                {
                    new TracksItemViewModel
                    {
                        Title = "Monoliths",
                        Position = "A1",
                        Rating = 5,
                        Artist = "Karkara",
                        Release = "All Is Dust",
                        DiscogsArtistId=1,
                        DiscogsReleaseId=2,
                    },
                    new TracksItemViewModel
                    {
                        Title = "Camel Rider",
                        Position = "A2",
                        Rating = 5,
                        Artist = "Karkara",
                        Release = "All Is Dust",
                        DiscogsArtistId=1,
                        DiscogsReleaseId=2,
                    },
                },
                CoverImage = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                IsFavourited = true
            };
            var release3 = new ReleaseViewModel
            {
                Year = "2023",
                OriginalReleaseYear = "2015",
                Title = "Revival",
                Artist = "Katchafire",
                ReleaseNotes = "Exceltante",
                ReleaseCountry = "New Zealand",
                DiscogsArtistId = 2,
                DiscogsReleaseId = 3,
                Genres = new List<(string? Name, int Id)>()
                {
                    new("Reggae", 3),
                    new("Dub", 4)
                },
                DiscogsReleaseUrl = "www.reggae.com",
                Tracks = new List<TracksItemViewModel>
                {
                    new TracksItemViewModel
                    {
                        Title = "Collie Herb Man",
                        Position = "A6",
                        Rating = 5,
                        Artist = "Katchafire",
                        Release = "Revival",
                        DiscogsArtistId=2,
                        DiscogsReleaseId=3,
                    },
                    new TracksItemViewModel
                    {
                        Title = "Sensimillia",
                        Position = "B1",
                        Rating = 5,
                        Artist = "Katchafire",
                        Release = "Revival",
                        DiscogsArtistId=2,
                        DiscogsReleaseId=3,
                    },
                },
                CoverImage = new byte[] { 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                IsFavourited = true
            };


            return new List<ReleaseViewModel>
            {
                release1,release2,release3
            };
        }

        /// <summary>
        /// Both items in list share a same genre
        /// </summary>
        /// <returns></returns>
        public static List<ReleaseViewModel> GetSampleReleaseViewModelListBySharedGenre()
        {
            var release1 = new ReleaseViewModel
            {
                Year = "2019",
                OriginalReleaseYear = "2019",
                Title = "Crystal Gazer",
                Artist = "Karkara",
                ReleaseNotes = "Exceltante",
                ReleaseCountry = "France",
                DiscogsArtistId = 100,
                DiscogsReleaseId = 100,
                Genres = new List<(string? Name, int Id)>()
                {
                    new("Psych", 1),
                    new("Rock", 2),
                    new("Alternative", 3)
                },
                DiscogsReleaseUrl = "www.psych.com",
                Tracks = new List<TracksItemViewModel>
                {
                    new TracksItemViewModel
                    {
                        Title = "Proxima Centaury",
                        Position = "A1",
                        Rating = 5,
                        Artist = "Karkara",
                        Release = "Crystal Gazer",
                        DiscogsArtistId=1,
                        DiscogsReleaseId=1,
                    },
                    new TracksItemViewModel
                    {
                        Title = "Camel Rider",
                        Position = "A2",
                        Rating = 5,
                        Artist = "Karkara",
                        Release = "Crystal Gazer",
                        DiscogsArtistId=1,
                        DiscogsReleaseId=1,
                    },
                },
                CoverImage = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                IsFavourited = true
            };

            var release3 = new ReleaseViewModel
            {
                Year = "2023",
                OriginalReleaseYear = "2015",
                Title = "Revival",
                Artist = "Katchafire",
                ReleaseNotes = "Exceltante",
                ReleaseCountry = "New Zealand",
                DiscogsArtistId = 300,
                DiscogsReleaseId = 300,
                Genres = new List<(string? Name, int Id)>()
                {
                    new("Reggae", 3),
                    new("Dub", 4),
                    new("Alternative", 3)
                },
                DiscogsReleaseUrl = "www.reggae.com",
                Tracks = new List<TracksItemViewModel>
                {
                    new TracksItemViewModel
                    {
                        Title = "Collie Herb Man",
                        Position = "A6",
                        Rating = 5,
                        Artist = "Katchafire",
                        Release = "Revival",
                        DiscogsArtistId=2,
                        DiscogsReleaseId=3,
                    },
                    new TracksItemViewModel
                    {
                        Title = "Sensimillia",
                        Position = "B1",
                        Rating = 5,
                        Artist = "Katchafire",
                        Release = "Revival",
                        DiscogsArtistId=2,
                        DiscogsReleaseId=3,
                    },
                },
                CoverImage = new byte[] { 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                IsFavourited = true
            };

            return new List<ReleaseViewModel>
            {
                release1,release3
            };
        }
    }
}
