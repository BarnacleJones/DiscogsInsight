using DiscogsInsight.ApiIntegration.Contract;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.Database.Contract;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

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
    }
}
