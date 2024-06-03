using DiscogsInsight.ApiIntegration.Contract;
using DiscogsInsight.ApiIntegration.Models.DiscogsResponseModels;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.Database.Contract;
using Microsoft.Extensions.Logging;
using Moq;

namespace DiscogsInsight.DataAccess.Tests
{
    public class CollectionDataServiceTests
    {
        private Mock<IDiscogsInsightDb> _dbMock;
        private Mock<IDiscogsApiService> _discogsApiServiceMock;
        private Mock<IDiscogsGenresAndTagsDataService> _genresAndTagsDataServiceMock;
        private Mock<ILogger<CollectionDataService>> _loggerMock;
        private CollectionDataService _service;

        [SetUp]
        public void Setup()
        {
            _dbMock = new Mock<IDiscogsInsightDb>();
            _discogsApiServiceMock = new Mock<IDiscogsApiService>();
            _genresAndTagsDataServiceMock = new Mock<IDiscogsGenresAndTagsDataService>();
            _loggerMock = new Mock<ILogger<CollectionDataService>>();

            _service = new CollectionDataService(_dbMock.Object, _discogsApiServiceMock.Object, _loggerMock.Object, _genresAndTagsDataServiceMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _service = null;
        }

        [Test]
        public async Task GetArtistsIdsAndNames_ReturnsCorrectData()
        {
            // Arrange
            var artists = new List<DiscogsInsight.Database.Entities.Artist>
            {
                new DiscogsInsight.Database.Entities.Artist { DiscogsArtistId = 1, Name = "Artist 1" },
                new DiscogsInsight.Database.Entities.Artist { DiscogsArtistId = 2, Name = "Artist 2" }
            };
            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<DiscogsInsight.Database.Entities.Artist>()).ReturnsAsync(artists);

            // Act
            var result = await _service.GetArtistsIdsAndNames();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].DiscogsArtistId, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Artist 1"));
        }

        [Test]
        public async Task GetReleases_ReturnsCorrectData()
        {
            // Arrange
            var releases = new List<DiscogsInsight.Database.Entities.Release>
            {
                new DiscogsInsight.Database.Entities.Release { DiscogsReleaseId = 1, Title = "Release 1" },
                new DiscogsInsight.Database.Entities.Release { DiscogsReleaseId = 2, Title = "Release 2" }
            };
            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<DiscogsInsight.Database.Entities.Release>()).ReturnsAsync(releases);

            // Act
            var result = await _service.GetReleases();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].DiscogsReleaseId, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Release 1"));
        }

        [Test]
        public async Task CollectionSavedOrUpdatedFromDiscogs_ReturnsFalse_WhenDataIsNull()
        {
            // Arrange
            _discogsApiServiceMock.Setup(api => api.GetCollectionFromDiscogsApi()).ReturnsAsync((DiscogsCollectionResponse)null);

            // Act
            var result = await _service.CollectionSavedOrUpdatedFromDiscogs();

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void CollectionSavedOrUpdatedFromDiscogs_ThrowsException_WhenApiCallFails()
        {
            // Arrange
            _discogsApiServiceMock.Setup(api => api.GetCollectionFromDiscogsApi()).ThrowsAsync(new System.Exception("API error"));

            // Act & Assert
            Assert.ThrowsAsync<System.Exception>(async () => await _service.CollectionSavedOrUpdatedFromDiscogs());
        }
    }
}
