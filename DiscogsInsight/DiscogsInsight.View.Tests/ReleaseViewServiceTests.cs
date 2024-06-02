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
        private Mock<ITracksDataService> _tracksDataServiceMock;
        private Mock<IDiscogsGenresAndTagsDataService> _discogsGenresAndTagsDataService;


        [SetUp]
        public void Setup()
        {
            _releaseDataServiceMock = new Mock<IReleaseDataService>();
            _artistDataServiceMock = new Mock<IArtistDataService>();
            _tracksDataServiceMock = new Mock<ITracksDataService>();
            _discogsGenresAndTagsDataService = new Mock<IDiscogsGenresAndTagsDataService>();
        }

        [Test]
        public async Task GetRelease_WithValidDiscogsReleaseId_ReturnsValidReleaseViewModel()
        {
            // Arrange
            int discogsReleaseId;
            ReleaseViewModel expectedRelease;
            byte[] expectedCoverImage;
            ArrangeGetReleaseData(out discogsReleaseId, out expectedRelease, out expectedCoverImage);

            var releaseViewService = new ReleaseViewService(_releaseDataServiceMock.Object, _artistDataServiceMock.Object, _tracksDataServiceMock.Object, _discogsGenresAndTagsDataService.Object);

            // Act
            var result = await releaseViewService.GetRelease(discogsReleaseId);

            // Assert
            Assert.That(result.Data.DiscogsArtistId, Is.EqualTo(expectedRelease.DiscogsArtistId));
            Assert.That(result.Data.CoverImage, Is.EqualTo(expectedCoverImage));
        }

        private void ArrangeGetReleaseData(out int discogsReleaseId, out ReleaseViewModel expectedRelease, out byte[] expectedCoverImage)
        {
            discogsReleaseId = 123;
            expectedRelease = new ReleaseViewModel() { DiscogsArtistId = 1 };
            var expectedReleaseData = new Release() { DiscogsArtistId = 1, DiscogsReleaseId = 123 }; // Set your expected release here
            var expectedArtistData = new DiscogsInsight.Database.Entities.Artist() { DiscogsArtistId = 1, Name = "Cindy Lee" };
            var expectedTracksData = new List<Track>() { new Track() { DiscogsArtistId = 1, Title = "Stone Faces" } };
            expectedCoverImage = new byte[10];

            //arrange data services
            _releaseDataServiceMock.Setup(x => x.GetReleaseAndImageAndRetrieveAllApiDataForRelease(123))
                .ReturnsAsync((expectedReleaseData, expectedCoverImage));

            _artistDataServiceMock.Setup(x => x.GetArtist(1, false)).ReturnsAsync(expectedArtistData);

            _tracksDataServiceMock.Setup(x => x.GetTracksForRelease(123)).ReturnsAsync(expectedTracksData);
        }

    }

}