using Moq;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.Database.Contract;
using DiscogsInsight.Database.Entities;
using DiscogsInsight.ApiIntegration.Models.DiscogsResponseModels;
using Microsoft.Extensions.Logging;

namespace DiscogsInsight.DataAccess.Services.Tests
{
    [TestFixture]
    public class DiscogsGenresAndTagsDataServiceTests
    {
        private Mock<IDiscogsInsightDb> _mockDb;
        private Mock<ILogger<DiscogsGenresAndTagsDataService>> _mockLogger;
        private Mock<ITagsDataService> _mockTagsDataService;
        private DiscogsGenresAndTagsDataService _service;

        [SetUp]
        public void Setup()
        {
            _mockDb = new Mock<IDiscogsInsightDb>();
            _mockLogger = new Mock<ILogger<DiscogsGenresAndTagsDataService>>();
            _mockTagsDataService = new Mock<ITagsDataService>();
            _service = new DiscogsGenresAndTagsDataService(_mockDb.Object, _mockLogger.Object, _mockTagsDataService.Object);
        }

        [Test]
        public async Task SaveGenresFromDiscogsRelease_ReturnsTrue_WhenGenresAreSaved()
        {
            // Arrange
            var responseRelease = new ResponseRelease
            {
                basic_information = new BasicInformation
                {
                    genres = new List<string> { "Rock", "Pop" }
                }
            };
            var discogsReleaseId = 1;
            var discogsArtistId = 1;

            var existingTags = new List<DiscogsGenreTags>
            {
                new DiscogsGenreTags { Id = 1, DiscogsTag = "Rock" }
            };

            _mockDb.Setup(db => db.GetAllEntitiesAsListAsync<DiscogsGenreTags>())
                .ReturnsAsync(existingTags);

            _mockDb.Setup(db => db.InsertAsync(It.IsAny<DiscogsGenreTags>()))
                .ReturnsAsync(1);

            _mockDb.Setup(db => db.InsertAsync(It.IsAny<DiscogsGenreTagToDiscogsRelease>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.SaveGenresFromDiscogsRelease(responseRelease, discogsReleaseId, discogsArtistId);

            // Assert
            Assert.That(result, Is.True);
            _mockDb.Verify(db => db.InsertAsync(It.Is<DiscogsGenreTags>(tag => tag.DiscogsTag == "Pop")), Times.Once);
            _mockDb.Verify(db => db.InsertAsync(It.Is<DiscogsGenreTagToDiscogsRelease>(rel => rel.DiscogsGenreTagId == 1 && rel.DiscogsReleaseId == discogsReleaseId && rel.DiscogsArtistId == discogsArtistId)), Times.Once);
        }

        [Test]
        public async Task SaveGenresFromDiscogsRelease_ReturnsTrue_WhenNoGenresToSave()
        {
            // Arrange
            var responseRelease = new ResponseRelease
            {
                basic_information = new BasicInformation
                {
                    genres = new List<string>()
                }
            };
            var discogsReleaseId = 1;
            var discogsArtistId = 1;

            var existingTags = new List<DiscogsGenreTags>();

            _mockDb.Setup(db => db.GetAllEntitiesAsListAsync<DiscogsGenreTags>())
                .ReturnsAsync(existingTags);

            // Act
            var result = await _service.SaveGenresFromDiscogsRelease(responseRelease, discogsReleaseId, discogsArtistId);

            // Assert
            Assert.That(result, Is.True);
            _mockDb.Verify(db => db.InsertAsync(It.IsAny<DiscogsGenreTags>()), Times.Never);
            _mockDb.Verify(db => db.InsertAsync(It.IsAny<DiscogsGenreTagToDiscogsRelease>()), Times.Never);
        }

        [Test]
        public async Task SaveGenresFromDiscogsRelease_ReturnsTrue_WhenResponseIsNull()
        {
            // Arrange
            ResponseRelease responseRelease = null;
            var discogsReleaseId = 1;
            var discogsArtistId = 1;

            // Act
            var result = await _service.SaveGenresFromDiscogsRelease(responseRelease, discogsReleaseId, discogsArtistId);

            // Assert
            Assert.That(result, Is.True);
            _mockDb.Verify(db => db.InsertAsync(It.IsAny<DiscogsGenreTags>()), Times.Never);
            _mockDb.Verify(db => db.InsertAsync(It.IsAny<DiscogsGenreTagToDiscogsRelease>()), Times.Never);
        }

    }
}
