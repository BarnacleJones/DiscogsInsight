﻿using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.ViewModels.EntityViewModels;
using DiscogsInsight.ViewModels.Results;

namespace DiscogsInsight.View.Services.Artist
{
    public class ArtistViewService
    {
        private readonly ArtistDataService _artistDataService;
        private readonly TagsDataService _tagsDataService;
        public ArtistViewService(ArtistDataService artistDataService, TagsDataService tagsDataService)
        {
            _artistDataService = artistDataService;
            _tagsDataService = tagsDataService; 
        }

        public async Task<ViewResult<ArtistViewModel>> GetArtist(int? discogsArtistId)
        {
            try
            {
                var artist = await _artistDataService.GetArtist(discogsArtistId);

                var tags = await _tagsDataService.GetTagsByMusicBrainzArtistId(artist.MusicBrainzArtistId);
                var tagsList = tags.Select(x => x.Tag).ToList();
                
                var releasesByThisArtist = await _artistDataService.GetArtistsReleasesByMusicBrainzArtistId(artist.MusicBrainzArtistId);

                var releasesViewModel = releasesByThisArtist.GroupBy(x => x.Status)
                    .Select(x => new MusicBrainzArtistsReleasesViewModel
                    {
                        Status = x.Key,
                        Releases = x.OrderBy(y => y.ReleaseYear)
                                    .ThenBy(y => y.MusicBrainzReleaseName)
                                    .Select(y => new MusicBrainzReleaseViewModel
                                    {
                                        Title = y.MusicBrainzReleaseName,
                                        Year = y.ReleaseYear
                                    }).ToList()
                    }).ToList();

                var data = new ArtistViewModel
                {
                    Artist = artist.Name,
                    ArtistDescription = artist.Profile,
                    DiscogsArtistId = artist.DiscogsArtistId,
                    City = artist.City,
                    Country = artist.Country,  
                    StartYear = artist.StartYear,
                    EndYear = artist.EndYear,
                    MusicBrainzArtistId = artist.MusicBrainzArtistId,
                    Tags = tagsList,
                    ArtistsReleases = releasesViewModel
                };

                return new ViewResult<ArtistViewModel>
                {
                    Data = data,
                    ErrorMessage = "",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new ViewResult<ArtistViewModel>
                {
                    Data = null,
                    ErrorMessage = ex.Message,
                    Success = false
                };
            }
        }

    }
}
