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
    public class ReleaseDataServiceTests
    {
        private Mock<IDiscogsInsightDb> _dbMock;
        private Mock<ILogger<ReleaseDataService>> _loggerMock;
        private Mock<ReleaseDataService> _service;

        private Mock<IMusicBrainzApiService>_musicBrainzApiServiceMock;
        private Mock<IDiscogsApiService>_discogsApiServiceMock;
        private Mock<ICoverArtArchiveApiService>_coverArchiveApiServiceMock;
        private Mock<ICollectionDataService>_collectionDataServiceMock;
        private Mock<IArtistDataService>_artistDataServiceMock;
        private Mock<IDiscogsGenresAndTagsDataService>_discogsGenresAndTagsDataServiceMock;
        
        
        [SetUp]
        public void Setup()
        {
            _dbMock = new Mock<IDiscogsInsightDb>();
            _musicBrainzApiServiceMock = new Mock<IMusicBrainzApiService>();
            _discogsApiServiceMock = new Mock<IDiscogsApiService>();
            _collectionDataServiceMock = new Mock<ICollectionDataService>();
            _artistDataServiceMock = new Mock<IArtistDataService>();
            _coverArchiveApiServiceMock = new Mock<ICoverArtArchiveApiService>();
            _discogsGenresAndTagsDataServiceMock = new Mock<IDiscogsGenresAndTagsDataService>();
            _loggerMock = new Mock<ILogger<ReleaseDataService>>();
            _service = new Mock<ReleaseDataService>(_dbMock.Object, _musicBrainzApiServiceMock.Object, _discogsGenresAndTagsDataServiceMock.Object, _artistDataServiceMock.Object, _discogsApiServiceMock.Object, _collectionDataServiceMock.Object, _coverArchiveApiServiceMock.Object, _loggerMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _service = null;
        }


        [Test]
        public async Task SetFavouriteBooleanOnRelease_Success()
        {
            // Arrange
            var discogsReleaseId = 100;
            var releases = DatabaseDataGenerator.GetSampleReleases();
            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<Release>())
                .ReturnsAsync(releases);

            // Act
            var result = await _service.Object.SetFavouriteBooleanOnRelease(true, discogsReleaseId);
            // Assert
            Assert.That(result, Is.True);
            _dbMock.Verify(db => db.SaveItemAsync(It.IsAny<Release>()), Times.Once);
        }

        // Test cases for GetAllReleasesAsList method
        [Test]
        public async Task GetAllReleasesAsList_ReturnsList()
        {
            // Arrange
            var releases = DatabaseDataGenerator.GetSampleReleases().ToList();
            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<Release>())
                .ReturnsAsync(releases);

            // Act
            var result = await _service.Object.GetAllReleasesAsList();

            // Assert
            Assert.That(releases, Is.EqualTo(result));
        }

        [Test]
        public async Task GetAllDiscogsReleaseIdsForArtist_ReturnsReleaseIds()
        {
            // Arrange
            var discogsArtistId = 123;
            var releases = new List<Release>
            {
                new Release { DiscogsReleaseId = 1, DiscogsArtistId = discogsArtistId },
                new Release { DiscogsReleaseId = 2, DiscogsArtistId = discogsArtistId },
                new Release { DiscogsReleaseId = 3, DiscogsArtistId = discogsArtistId },
                new Release { DiscogsReleaseId = 4, DiscogsArtistId = discogsArtistId },
                new Release { DiscogsReleaseId = 5, DiscogsArtistId = 6 },
            };
            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<Release>())
                .ReturnsAsync(releases);

            // Act
            var result = await _service.Object.GetAllDiscogsReleaseIdsForArtist(discogsArtistId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result.Contains(1), Is.True);
            Assert.That(result.Contains(2), Is.True);
            Assert.That(result.Contains(3), Is.True);
            Assert.That(result.Contains(4), Is.True);
            Assert.That(result.Contains(5), Is.False);
        }

        //Intentionally leaving this as the most basic test case - the rest of this function will be rewritten
        [Test]
        public async Task GetReleaseAndImageAndRetrieveAllApiDataForRelease_WithValidId_ReturnsReleaseAndCoverImage()
        {
            // Arrange
            int discogsReleaseId = 100;
            var release = DatabaseDataGenerator.GetSampleReleases().Where(x => x.DiscogsReleaseId == 100).First();
            var artists = DatabaseDataGenerator.GetSampleArtists();
            var musicBrainzReleaseToCoverImages = DatabaseDataGenerator.GetSampleMusicBrainzReleaseToCoverImages();
            var discogsReleaseResponse = ApiDataGenerator.GetSampleDiscogsReleaseResponse();
            var coverImageBytes = new byte[] { 0,0,0,0 };

            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<Release>())
                .ReturnsAsync(new List<Release> { release });

            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<Artist>())
                .ReturnsAsync(artists);

            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<MusicBrainzReleaseToCoverImage>())
                .ReturnsAsync(musicBrainzReleaseToCoverImages);

            _discogsApiServiceMock.Setup(service => service.GetReleaseFromDiscogs(It.IsAny<int>()))
                .ReturnsAsync(discogsReleaseResponse);

            _coverArchiveApiServiceMock.Setup(service => service.GetCoverByteArray(It.IsAny<string>()))
                .ReturnsAsync(coverImageBytes);

            // Act
            var result = await _service.Object.GetReleaseAndImageAndRetrieveAllApiDataForRelease(discogsReleaseId);

            // Assert
            Assert.That(result.Item1, Is.Not.Null);
            Assert.That(result.Item2, Is.Not.Null);
        }

        [Test]
        public async Task GetImageForRelease_WithValidId_ReturnsCoverImage()
        {
            // Arrange
            string musicBrainzReleaseId = "12345";
            var coverImageBytes = new byte[] {  };
            var musicBrainzReleaseToCoverImage = new MusicBrainzReleaseToCoverImage { MusicBrainzReleaseId = musicBrainzReleaseId, MusicBrainzCoverImage = coverImageBytes };

            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<MusicBrainzReleaseToCoverImage>())
                .ReturnsAsync(new List<MusicBrainzReleaseToCoverImage> { musicBrainzReleaseToCoverImage });

            // Act
            var result = await _service.Object.GetImageForRelease(musicBrainzReleaseId);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        //[Test]
        //public async Task GetRandomRelease_WithValidData_ReturnsReleaseAndCoverImage()
        //{
        //    // Arrange
        //    var release = new Release { DiscogsReleaseId = 123, DiscogsArtistId = 456, Title = "Test Release" };
        //    var coverImageBytes = new byte[] {  };

        //    _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<Release>())
        //        .ReturnsAsync(new List<Release> { release });

        //    _service.Setup(service => service.GetReleaseAndImageAndRetrieveAllApiDataForRelease(It.IsAny<int>()))
        //        .ReturnsAsync((release, coverImageBytes));

        //    // Act
        //    var result = await _service.Object.GetRandomRelease();

        //    // Assert
        //    Assert.That(result.Item1, Is.Not.Null);
        //    Assert.That(result.Item2, Is.Not.Null);
        //    // Add more assertions as needed
        //}

        //[Test]
        //public async Task GetNewestReleases_WithValidData_ReturnsNewestReleases()
        //{
        //    // Arrange
        //    int howManyToReturn = 5;
        //    var release = new Release { DiscogsReleaseId = 123, DiscogsArtistId = 456, Title = "Test Release" };
        //    var coverImageBytes = new byte[] {  };

        //    _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<Release>())
        //        .ReturnsAsync(new List<Release> { release });

        //    _service.Setup(service => service.GetReleaseAndImageAndRetrieveAllApiDataForRelease(It.IsAny<int>()))
        //        .ReturnsAsync((release, coverImageBytes));

        //    // Act
        //    var result = await _service.Object.GetNewestReleases(howManyToReturn);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(howManyToReturn, Is.EqualTo(result.Count));
        //}

        //[Test]
        //public async Task GetPossibleReleasesForDataCorrectionFromDiscogsReleaseId_WithValidId_ReturnsPossibleReleases()
        //{
        //    // Arrange
        //    int? discogsReleaseId = 12345;
        //    var allReleasesKnownByArtistId = ("badMusicBrainzReleaseId", new List<PossibleReleasesFromArtist>
        //    {
        //        new PossibleReleasesFromArtist { Date = "2022", MusicBrainzReleaseId = "mb1", Status = "Status1", Title = "Title1" },
        //        new PossibleReleasesFromArtist { Date = "2023", MusicBrainzReleaseId = "mb2", Status = "Status2", Title = "Title2" }
        //    });

        //    var savedCoverImages = new List<MusicBrainzReleaseToCoverImage>
        //    {
        //        new MusicBrainzReleaseToCoverImage { MusicBrainzReleaseId = "mb1", MusicBrainzCoverImage = new byte[] {} }
        //    };

        //    _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<Release>())
        //        .ReturnsAsync(new List<Release> { new Release { DiscogsReleaseId = discogsReleaseId, MusicBrainzReleaseId = "badMusicBrainzReleaseId" } });

        //    _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<MusicBrainzReleaseToCoverImage>())
        //        .ReturnsAsync(savedCoverImages);

        //    _service.Setup(service => service.GetAllStoredMusicBrainzReleasesForArtistByDiscogsReleaseId(discogsReleaseId))
        //        .ReturnsAsync(allReleasesKnownByArtistId);

        //    _dbMock.Setup(db => db.DeleteAsync(It.IsAny<MusicBrainzReleaseToCoverImage>()))
        //        .ReturnsAsync(1);

        //    // Act
        //    var result = await _service.Object.GetPossibleReleasesForDataCorrectionFromDiscogsReleaseId(discogsReleaseId);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //}
    }

}
