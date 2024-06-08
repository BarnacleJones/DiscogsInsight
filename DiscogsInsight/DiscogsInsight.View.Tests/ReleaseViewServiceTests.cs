using DiscogsInsight.Database.Entities;
using DiscogsInsight.Service.Releases;
using Moq;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.Service.Models.EntityViewModels;

namespace DiscogsInsight.Service.Tests
{
    [TestFixture]
    public class ReleaseViewServiceTests
    {
        private Mock<IReleaseDataService> _releaseDataServiceMock;
        private Mock<IArtistDataService> _artistDataServiceMock;
        private ReleaseViewService _service;


        [SetUp]
        public void Setup()
        {
            _releaseDataServiceMock = new Mock<IReleaseDataService>();
            _artistDataServiceMock = new Mock<IArtistDataService>();

            //Default Data Service List calls
            StageDummyData();

            _service = new ReleaseViewService(_releaseDataServiceMock.Object, _artistDataServiceMock.Object);
        }

        private void StageDummyData()
        {
            //var releases = DatabaseDataGenerator.GetSampleReleases();
            //_releaseDataServiceMock.Setup(s => s.GetAllReleasesAsList())
            //    .ReturnsAsync(releases);

            //var artist = DatabaseDataGenerator.GetSampleArtists().Where(x => x.DiscogsArtistId == 300).First();
            //_artistDataServiceMock.Setup(s => s.GetArtistByDiscogsId(300, true)).ReturnsAsync(artist);

            //var image = new byte[] {0,0,0,0,0,0,0,0,0,0,0,0,0 };
            //_releaseDataServiceMock.Setup(s => s.GetImageForRelease("1")).ReturnsAsync(image);
        }

        [Test]
        public async Task GetRelease_WithValidDiscogsReleaseId_ReturnsValidReleaseViewModel()
        {
            // Arrange
            int discogsReleaseId;
            ReleaseViewModel expectedRelease;
            byte[] expectedCoverImage;
            ArrangeGetReleaseData(out discogsReleaseId, out expectedRelease, out expectedCoverImage);

            // Act
            var result = _service.GetRelease(discogsReleaseId).Result;

            // Assert
            Assert.That(result.Data.DiscogsArtistId, Is.EqualTo(expectedRelease.DiscogsArtistId));
            Assert.That(result.Data.CoverImage, Is.EqualTo(expectedCoverImage));
        }

        [Test]
        public async Task GetReleasesByGenreId_ReturnsValidData()
        {
            //Arrange
            var genresAndIds = new List<(string? Name, int Id)>()
                {
                    new("Psych", 1),
                    new("Rock", 2),
                    new("Alternative", 3)
                };

            //Act
            var result = _service.GetReleasesByDiscogsGenreTagId(300).Result;

            //Assert
            Assert.That(result.Data.Count, Is.EqualTo(1));
            Assert.That(result.Data.Any(x => x.DiscogsReleaseId == 300), Is.EqualTo(true));
            Assert.That(result.Data.Any(x => x.DiscogsArtistId == 300), Is.EqualTo(true));
            Assert.That(result.Data.Any(x => x.Title == "Release 3"), Is.EqualTo(true));
        }

        private void ArrangeGetReleaseData(out int discogsReleaseId, out ReleaseViewModel expectedRelease, out byte[] expectedCoverImage)
        {
            discogsReleaseId = 123;
            expectedRelease = new ReleaseViewModel() { DiscogsArtistId = 1 };
            var expectedReleaseData = new Release() { DiscogsArtistId = 1, DiscogsReleaseId = 123 };
            var expectedArtistData = new DiscogsInsight.Database.Entities.Artist() { DiscogsArtistId = 1, Name = "Cindy Lee" };
            var expectedTracksData = new List<Track>() { new Track() { DiscogsArtistId = 1, Title = "Stone Faces" } };
            expectedCoverImage = new byte[10];

            //_releaseDataServiceMock.Setup(x => x.GetReleaseAndImageAndRetrieveAllApiDataForRelease(123))
            //    .ReturnsAsync((expectedReleaseData, expectedCoverImage));

            //_artistDataServiceMock.Setup(x => x.GetArtistByDiscogsId(1, false)).ReturnsAsync(expectedArtistData);

            //_tracksDataServiceMock.Setup(x => x.GetTracksForRelease(123)).ReturnsAsync(expectedTracksData);
        }

    }

}