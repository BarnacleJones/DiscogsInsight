using DiscogsInsight.ApiIntegration.Contract;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.Database.Contract;
using Microsoft.Extensions.Logging;
using Moq;

namespace DiscogsInsight.DataAccess.Tests
{
    public class ArtistDataServiceTests
    {
        private Mock<IDiscogsInsightDb> _dbMock;
        private Mock<IDiscogsApiService> _discogsApiServiceMock;
        private Mock<ILogger<ArtistDataService>> _loggerMock;
        private Mock<IMusicBrainzApiService> _musicBrainzApiServiceMock;
        private Mock<ITagsDataService> _tagsDataServiceMock;
        private ArtistDataService _service;

        [SetUp]
        public void Setup()
        {
            _dbMock = new Mock<IDiscogsInsightDb>();
            _discogsApiServiceMock = new Mock<IDiscogsApiService>();
            _loggerMock = new Mock<ILogger<ArtistDataService>>();
            _musicBrainzApiServiceMock = new Mock<IMusicBrainzApiService>();
            _tagsDataServiceMock = new Mock<ITagsDataService>();

            _service = new ArtistDataService(
                _dbMock.Object,
                _discogsApiServiceMock.Object,
                _loggerMock.Object,
                _musicBrainzApiServiceMock.Object,
                _tagsDataServiceMock.Object
            );
        }

        [TearDown]
        public void Teardown()
        {
            _service = null;
        }


        [Test]
        public async Task GetArtistByDiscogsId_ReturnsCorrectArtist()
        {
            // Arrange
            int discogsArtistId = 123;
            var expectedArtist = new Database.Entities.Artist { DiscogsArtistId = discogsArtistId };
            _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<Database.Entities.Artist>()).ReturnsAsync(new List<Database.Entities.Artist> { expectedArtist });

            // Act
            var result = await _service.GetArtistByDiscogsId(discogsArtistId);

            // Assert
            Assert.That(expectedArtist, Is.EqualTo(result));
        }

        [Test]
        public async Task GetArtistByDiscogsId_WithNoMatchingArtist_ReturnsNull()
        {
            // Arrange
            int discogsArtistId = 123;
            _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<Database.Entities.Artist>()).ReturnsAsync(new List<Database.Entities.Artist>());

            // Act
            var result = await _service.GetArtistByDiscogsId(discogsArtistId);

            // Assert
            Assert.That(result, Is.Null);
        }

        //[Test]
        //public async Task GetArtist_WithValidData_ReturnsArtist()
        //{
        //    // Arrange
        //    int discogsArtistId = 123;
        //    var expectedArtist = new Database.Entities.Artist { DiscogsArtistId = discogsArtistId };
        //    _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<Database.Entities.Artist>()).ReturnsAsync(new List<Database.Entities.Artist> { expectedArtist });

        //    // Act
        //    var result = await _service.GetArtist(discogsArtistId, fetchAndSaveApiData: true);

        //    // Assert
        //    Assert.That(expectedArtist, Is.EqualTo(result));
        //}

        //[Test]
        //public async Task GetArtistsReleasesByMusicBrainzArtistId_ReturnsListOfReleases()
        //{
        //    // Arrange
        //    string musicBrainzArtistId = "123";
        //    var expectedReleases = new List<MusicBrainzArtistRelease> { new MusicBrainzArtistRelease(), new MusicBrainzArtistRelease() };
        //    _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<MusicBrainzArtistToMusicBrainzRelease>()).ReturnsAsync(new List<MusicBrainzArtistToMusicBrainzRelease>());
        //    _service = new ArtistDataService(_dbMock.Object, _discogsApiServiceMock.Object, _loggerMock.Object, _musicBrainzApiServiceMock.Object, _tagsDataServiceMock.Object);
         
        //    // Act
        //    var result = await _service.GetArtistsReleasesByMusicBrainzArtistId(musicBrainzArtistId);

        //    // Assert
        //    Assert.That(expectedReleases, Is.EqualTo(result));
        //}

        //[Test]
        //public async Task GetPossibleArtistsForDataCorrectionFromDiscogsReleaseId_ReturnsListOfPossibleArtists()
        //{
        //    // Arrange
        //    int discogsReleaseId = 123;
        //    var expectedPossibleArtists = new List<PossibleArtistsFromMusicBrainzApi> { new PossibleArtistsFromMusicBrainzApi(), new PossibleArtistsFromMusicBrainzApi() };
        //    _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<Database.Entities.Release>()).ReturnsAsync(new List<Database.Entities.Release>());
        //    _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<Database.Entities.Artist>()).ReturnsAsync(new List<Database.Entities.Artist>());
        //    _musicBrainzApiServiceMock.Setup(m => m.GetInitialArtistFromMusicBrainzApi(It.IsAny<string>())).ReturnsAsync(new MusicBrainzInitialArtist());

        //    // Act
        //    var result = await _service.GetPossibleArtistsForDataCorrectionFromDiscogsReleaseId(discogsReleaseId);

        //    // Assert
        //    Assert.That(expectedPossibleArtists, Is.EqualTo(result));
        //}

        //[Test]
        //public async Task DeleteExistingArtistDataAndUpdateToChosenMusicBrainzArtistFromMusicBrainzId_ReturnsTrue()
        //{
        //    // Arrange
        //    int discogsReleaseId = 123;
        //    string newArtistMusicBrainzId = "456";
        //    _musicBrainzApiServiceMock.Setup(m => m.GetArtistFromMusicBrainzApiUsingArtistId(newArtistMusicBrainzId)).ReturnsAsync(new MusicBrainzArtist());
        //    _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<Database.Entities.Release>()).ReturnsAsync(new List<Database.Entities.Release>());
        //    _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<Database.Entities.Artist>()).ReturnsAsync(new List<Database.Entities.Artist>());
        //    _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<MusicBrainzArtistToMusicBrainzTags>()).ReturnsAsync(new List<MusicBrainzArtistToMusicBrainzTags>());
        //    _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<MusicBrainzArtistToMusicBrainzRelease>()).ReturnsAsync(new List<MusicBrainzArtistToMusicBrainzRelease>());
        //    _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<MusicBrainzReleaseToCoverImage>()).ReturnsAsync(new List<MusicBrainzReleaseToCoverImage>());

        //    // Act
        //    var result = await _service.DeleteExistingArtistDataAndUpdateToChosenMusicBrainzArtistFromMusicBrainzId(discogsReleaseId, newArtistMusicBrainzId);

        //    // Assert
        //    Assert.That(result, Is.True);
        //}
    }
}
