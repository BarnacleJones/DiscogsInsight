using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Services;
using Moq;

namespace DiscogsInsight.DataAccess.Tests
{
    public class SettingsDataServiceTests
    {
        private Mock<ICollectionDataService> _collectionDataServiceMock;
        private Mock<IPreferencesService> _preferencesServiceMock;
        private SettingsDataService _service;

        [SetUp]
        public void Setup()
        {
            _collectionDataServiceMock = new Mock<ICollectionDataService>();
            _preferencesServiceMock = new Mock<IPreferencesService>();

            _service = new SettingsDataService(_collectionDataServiceMock.Object, _preferencesServiceMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _service = null;
        }

        [Test]
        public async Task UpdateCollection_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            _collectionDataServiceMock.Setup(service => service.CollectionSavedOrUpdatedFromDiscogs()).ReturnsAsync(true);

            // Act
            var result = await _service.UpdateCollection();

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetDiscogsUsername_ReturnsCorrectUsername()
        {
            // Arrange
            var expectedUsername = "testuser";
            _preferencesServiceMock.Setup(prefs => prefs.Get(PreferencesConstant.DiscogsUsername, "")).Returns(expectedUsername);

            // Act
            var result = _service.GetDiscogsUsername();

            // Assert
            Assert.That(result, Is.EqualTo(expectedUsername));
        }

        [Test]
        public async Task UpdateDiscogsUsername_RemovesUsername_WhenEmpty()
        {
            // Act
            var result = await _service.UpdateDiscogsUsername(string.Empty);

            // Assert
            _preferencesServiceMock.Verify(prefs => prefs.Remove(PreferencesConstant.DiscogsUsername), Times.Once);
            _collectionDataServiceMock.Verify(service => service.PurgeEntireCollection(), Times.Once);
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task UpdateDiscogsUsername_SetsUsername_WhenNotEmpty()
        {
            // Arrange
            var newUsername = "newuser";

            // Act
            var result = await _service.UpdateDiscogsUsername(newUsername);

            // Assert
            _preferencesServiceMock.Verify(prefs => prefs.Set(PreferencesConstant.DiscogsUsername, newUsername), Times.Once);
            _collectionDataServiceMock.Verify(service => service.PurgeEntireCollection(), Times.Once);
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task PurgeEntireDb_CallsPurgeEntireDatabase()
        {
            // Act
            var result = await _service.PurgeEntireDb();

            // Assert
            _collectionDataServiceMock.Verify(service => service.PurgeEntireDatabase(), Times.Once);
            Assert.That(result, Is.True);
        }

    }
}
