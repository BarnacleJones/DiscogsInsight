using DiscogsInsight.ApiIntegration.Contract;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.Tests.Common;
using DiscogsInsight.Database.Contract;
using DiscogsInsight.Database.Entities;
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

            _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<Database.Entities.Artist>()).ReturnsAsync(DummyDatabaseDataGenerator.GetSampleArtists());
            _dbMock.Setup(db => db.UpdateAsync(It.IsAny<Artist>())).Returns(Task.FromResult(1));

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
            var expectedArtist = DummyDatabaseDataGenerator.GetSampleArtists().First();
            int discogsArtistId = expectedArtist.DiscogsArtistId.Value;
           
            // Act
            var result = await _service.GetArtistByDiscogsId(discogsArtistId);

            // Assert
            Assert.That(expectedArtist.DiscogsArtistId, Is.EqualTo(result.DiscogsArtistId));
        }

        [Test]
        public async Task GetArtistByDiscogsId_DiscogsDataIsFetchedAndArtistIsUpdated_WhenRequired()
        {
            //Arrange
            _musicBrainzApiServiceMock.Setup(m => m.GetInitialArtistFromMusicBrainzApi(It.IsAny<string>())).Returns(Task.FromResult(DummyApiDataGenerator.GetSampleMusicBrainzInitialArtistResponse()));
            _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<Database.Entities.Artist>()).ReturnsAsync(DummyDatabaseDataGenerator.GetSampleArtistsButOneNeedsDiscogsAndMusicBrainzData());
            var discogsArtistId = 500; //the 500 one will return null profile (triggering data retrieval)
            _discogsApiServiceMock.Setup(m => m.GetArtistFromDiscogs(It.IsAny<int>())).Returns(Task.FromResult(DummyApiDataGenerator.GetSampleDiscogsArtistResponse(discogsArtistId)));

            //Act
            var result = await _service.GetArtistByDiscogsId(discogsArtistId);

            //Assert
            _dbMock.Verify(db => db.UpdateAsync(It.IsAny<Artist>()), Times.Once);
            _discogsApiServiceMock.Verify(m => m.GetArtistFromDiscogs(It.IsAny<int>()), Times.Once);
            Assert.That(result.DiscogsArtistId, Is.EqualTo(discogsArtistId));
        }
        
        [Test]
        public async Task GetArtistByDiscogsId_DiscogsDataIsNotFetched_WhenNotRequired()
        {
            //Arrange
            _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<Database.Entities.Artist>()).ReturnsAsync(DummyDatabaseDataGenerator.GetSampleArtistsButOneNeedsDiscogsAndMusicBrainzData());
            var discogsArtistId = 500; //the 500 one will return null profile (triggering data retrieval)
            
            //Act
            var result = await _service.GetArtistByDiscogsId(discogsArtistId, false);

            //Assert
            _dbMock.Verify(db => db.UpdateAsync(It.IsAny<Artist>()), Times.Never);
            _discogsApiServiceMock.Verify(m => m.GetArtistFromDiscogs(It.IsAny<int>()), Times.Never);
            Assert.That(result.DiscogsArtistId, Is.EqualTo(discogsArtistId));
        }

        [Test]
        public async Task GetArtistByDiscogsId_ApiDataIsNotFetchedAndSaved_WhenArtistIsVarious()
        {
            //Arrange
            _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<Database.Entities.Artist>()).ReturnsAsync(DummyDatabaseDataGenerator.GetSampleArtistsButOneIsVarious());
            var discogsArtistId = 500; //the 500 one will return null profile (triggering data retrieval)
           
            //Act
            var result = await _service.GetArtistByDiscogsId(discogsArtistId);

            //Assert
            _dbMock.Verify(db => db.UpdateAsync(It.IsAny<Artist>()), Times.Never);
            _discogsApiServiceMock.Verify(m => m.GetArtistFromDiscogs(It.IsAny<int>()), Times.Never);
            _musicBrainzApiServiceMock.Verify(m => m.GetInitialArtistFromMusicBrainzApi(It.IsAny<string>()), Times.Never);
            Assert.That(result.DiscogsArtistId, Is.EqualTo(discogsArtistId));
        }

        [Test]
        public async Task GetArtistByDiscogsId_MusicBrainzDataIsFetchedAndSaved_WhenRequired()
        {
            //Arrange
            _musicBrainzApiServiceMock.Setup(m => m.GetInitialArtistFromMusicBrainzApi(It.IsAny<string>())).Returns(Task.FromResult(DummyApiDataGenerator.GetSampleMusicBrainzInitialArtistResponse()));
            _dbMock.Setup(m => m.GetAllEntitiesAsListAsync<Database.Entities.Artist>()).ReturnsAsync(DummyDatabaseDataGenerator.GetSampleArtistsButOneNeedsDiscogsAndMusicBrainzData());
            var discogsArtistId = 500; //the 500 one will return null profile and musicbrainzid (triggering data retrieval)
            _discogsApiServiceMock.Setup(m => m.GetArtistFromDiscogs(It.IsAny<int>())).Returns(Task.FromResult(DummyApiDataGenerator.GetSampleDiscogsArtistResponse(discogsArtistId)));

            //Act
            var result = await _service.GetArtistByDiscogsId(discogsArtistId);


            //Assert
            _dbMock.Verify(db => db.UpdateAsync(It.IsAny<Artist>()), Times.Once);
            _musicBrainzApiServiceMock.Verify(m => m.GetInitialArtistFromMusicBrainzApi(It.IsAny<string>()), Times.Once);

        }
    }
}
