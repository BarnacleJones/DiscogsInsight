﻿using DiscogsInsight.ApiIntegration.MusicBrainzResponseModels;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Entities;
using DiscogsInsight.DataAccess.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DiscogsInsight.DataAccess.Tests
{
    public class TagsDataServiceTests
    {
        private Mock<IDiscogsInsightDb> _dbMock;
        private Mock<ILogger<TagsDataService>> _loggerMock;
        private TagsDataService _service;

        [SetUp]
        public void Setup()
        {
            _dbMock = new Mock<IDiscogsInsightDb>();
            _loggerMock = new Mock<ILogger<TagsDataService>>();
            _service = new TagsDataService(_dbMock.Object, _loggerMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _service = null;
        }

        [Test]
        public async Task SaveTagsByMusicBrainzArtistId_SuccessfullySavesTags()
        {
            // Arrange
            var artistResponse = DummyDataGenerator.GetSampleMusicBrainzInitialArtistResponse();
            var tagsResponse = DummyDataGenerator.GetSampleMusicBrainzTags();
            var musicBrainzArtistId = "1"; // Example artist ID

            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<MusicBrainzTags>()).ReturnsAsync(tagsResponse);
            _dbMock.Setup(db => db.SaveItemAsync(It.IsAny<MusicBrainzTags>())).Returns(Task.FromResult(1));
            _dbMock.Setup(db => db.SaveItemAsync(It.IsAny<MusicBrainzArtistToMusicBrainzTags>())).Returns(Task.FromResult(1));

            // Act
            var result = await _service.SaveTagsByMusicBrainzArtistId(artistResponse, musicBrainzArtistId);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetTagsByMusicBrainzArtistId_ReturnsCorrectTags()
        {
            // Arrange
            var musicBrainzArtistId = "123"; // Example artist ID

            var tagsList = new List<MusicBrainzTags>(); // Mock your tags list
            var tagsToArtistsList = new List<MusicBrainzArtistToMusicBrainzTags>(); // Mock your tags to artists list

            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<MusicBrainzTags>()).ReturnsAsync(tagsList);
            _dbMock.Setup(db => db.GetAllEntitiesAsListAsync<MusicBrainzArtistToMusicBrainzTags>()).ReturnsAsync(tagsToArtistsList);

            // Act
            var result = await _service.GetTagsByMusicBrainzArtistId(musicBrainzArtistId);

            // Assert
            Assert.IsNotNull(result);
            // Add more assertions based on your test case
        }
    }
}