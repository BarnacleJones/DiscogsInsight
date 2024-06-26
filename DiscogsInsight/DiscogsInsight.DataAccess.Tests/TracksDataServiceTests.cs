//using DiscogsInsight.DataAccess.Contract;
//using DiscogsInsight.Database.Entities;
//using DiscogsInsight.DataAccess.Services;
//using Microsoft.Extensions.Logging;
//using Moq;
//using DiscogsInsight.Database.Contract;
//using DiscogsInsight.Tests.Common;

//namespace DiscogsInsight.DataAccess.Tests
//{
//    public class TracksDataServiceTests
//    {
//        private Mock<IDiscogsInsightDb> _dbMock;
//        private Mock<IReleaseDataService> _releaseDataServiceMock;
//        private TracksDataService _service;

//        [SetUp]
//        public void Setup()
//        {
//            _dbMock = new Mock<IDiscogsInsightDb>();
//            _releaseDataServiceMock = new Mock<IReleaseDataService>();
//            var mockLogger = new Mock<ILogger<TracksDataService>>();
//            _service = new TracksDataService(_dbMock.Object, mockLogger.Object, _releaseDataServiceMock.Object);
//        }

//        [TearDown]
//        public void Teardown()
//        {
//            _service = null;
//        }

//        [Test]
//        public async Task SetRatingOnTrack_DoesntThrowException()
//        {
//            // Arrange
//            var rating = 5;
//            var discogsReleaseId = 1;
//            var title = "Track 1";

//            var mockTracks = DatabaseDataGenerator.GetUniqueSampleTracks();
//            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<Track>()).ReturnsAsync(mockTracks);

//            // Act
//            var result = await _service.SetRatingOnTrack(rating, discogsReleaseId, title);

//            // Assert
//            Assert.That(result, Is.True);
//        }

//         [Test]
//        public async Task GetTracksForRelease_ReturnsCorrectTracks()
//        {
//            // Arrange
//            var discogsReleaseId = 100;

//            var mockTracks = DatabaseDataGenerator.GetUniqueSampleTracks();
//            var trackForRelease = new List<Track> { mockTracks[1] };
//            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<Track>()).ReturnsAsync(mockTracks);

//            var mockReleases = DatabaseDataGenerator.GetSampleReleases();
//            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<Release>()).ReturnsAsync(mockReleases);

//            // Act
//            var result = await _service.GetTracksForRelease(discogsReleaseId);

//            // Assert
//            Assert.That(trackForRelease, Is.EqualTo(result));
//        }

//        [Test]
//        public async Task GetAllTracksAsList_DoesThat()
//        {
//            // Arrange
           
//            var mockTracks = DatabaseDataGenerator.GetUniqueSampleTracks();
//            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<Track>()).ReturnsAsync(mockTracks);

//            // Act
//            var result = await _service.GetAllTracks();

//            // Assert
//            Assert.That(result, Is.EqualTo(mockTracks));
//        }
//    }
//}
