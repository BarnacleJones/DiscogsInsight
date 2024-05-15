using DiscogsInsight.ApiIntegration.Services;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DiscogsInsight.DataAccess.Tests
{
    public class ReleaseDataServiceTests
    {
        private Mock<IDiscogsInsightDb> _dbMock;
        private Mock<ILogger<ReleaseDataService>> _loggerMock;
        private ReleaseDataService _service;

        private Mock<MusicBrainzApiService>_musicBrainzApiServiceMock;
        private Mock<DiscogsApiService>_discogsApiServiceMock;
        private Mock<CoverArtArchiveApiService>_coverArchiveApiServiceMock;
        private Mock<ICollectionDataService>_collectionDataServiceMock;
        private Mock<IArtistDataService>_artistDataServiceMock;
        private Mock<IDiscogsGenresAndTagsDataService>_discogsGenresAndTagsDataServiceMock;
        
        
        [SetUp]
        public void Setup()
        {
            _dbMock = new Mock<IDiscogsInsightDb>();
            _loggerMock = new Mock<ILogger<ReleaseDataService>>();
            _service = new ReleaseDataService(_dbMock.Object, _musicBrainzApiServiceMock.Object, _discogsGenresAndTagsDataServiceMock.Object, _artistDataServiceMock.Object, _discogsGenresAndTagsDataServiceMock.Object, _collectionDataServiceMock.Object, _coverArchiveApiServiceMock.Object, _loggerMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _service = null;
        }
        
    }

}
