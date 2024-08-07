﻿using Microsoft.Extensions.Logging;
using DiscogsInsight.DataAccess.Services;
using MudBlazor.Services;
using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.ApiIntegration.Services;
using DiscogsInsight.ApiIntegration.Contract;
using DiscogsInsight.Service.Collection;
using DiscogsInsight.Service.Insights;
using DiscogsInsight.Database.Contract;
using DiscogsInsight.Database;
using DiscogsInsight.Service;
using IF.Lastfm.Core.Api;

namespace DiscogsInsight
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            //Http client factory
            //https://stackoverflow.com/questions/72288451/how-do-i-properly-use-di-with-ihttpclientfactory-in-net-maui

            builder.Services.AddHttpClient("DiscogsApiClient", hc => 
            {
                hc.BaseAddress = new Uri("https://api.discogs.com");
                hc.DefaultRequestHeaders.Add("User-Agent", "DiscogsInsight");
            });

            builder.Services.AddHttpClient("CoverArtApiClient", hc => 
            {
                hc.DefaultRequestHeaders.Add("User-Agent", $"DiscogsInsight");
                hc.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            builder.Services.AddHttpClient("MusicBrainzApiClient", hc => 
            {
                hc.BaseAddress = new Uri("https://musicbrainz.org");
                hc.DefaultRequestHeaders.Add("User-Agent", $"DiscogsInsight");
                hc.DefaultRequestHeaders.Add("Accept", "application/json");
            });
    
            //Dont need this for Windows but Android Requests must all be Https
            builder.Services.AddHttpClient("LastFmApiClient", hc => 
            {
                hc.BaseAddress = new Uri("https://ws.audioscrobbler.com/2.0/");
                hc.DefaultRequestHeaders.Add("Accept", "application/json");
            });
    

            //Database layer
            builder.Services.AddSingleton<ISQLiteAsyncConnection, SQLiteAsyncConnectionAdapter>();

            //Api layer
            builder.Services.AddSingleton<IDiscogsApiService, DiscogsApiService>();
            builder.Services.AddSingleton<IMusicBrainzApiService, MusicBrainzApiService>();
            builder.Services.AddSingleton<ICoverArtArchiveApiService, CoverArtArchiveApiService>();
            builder.Services.AddSingleton<ILastFmApiService, LastFmApiService>();

            //Data Access
            builder.Services.AddSingleton<ICollectionDataService,CollectionDataService>();
            builder.Services.AddSingleton<IArtistDataService,ArtistDataService>();
            builder.Services.AddSingleton<IArtistDataCorrectionService,ArtistDataCorrectionService>();
            builder.Services.AddSingleton<IReleaseDataService,ReleaseDataService>();
            builder.Services.AddSingleton<IPreferencesService,PreferencesService>();
            builder.Services.AddSingleton<ISettingsDataService, SettingsDataService>();
            builder.Services.AddSingleton<ITracksDataService,TracksDataService>();
            builder.Services.AddSingleton<IInsightsDataService, InsightsDataService>();

            //service layer
            builder.Services.AddSingleton<ArtistViewService>();
            builder.Services.AddSingleton<CollectionViewService>();
            builder.Services.AddSingleton<ReleaseViewService>();
            builder.Services.AddSingleton<SettingsViewService>();
            builder.Services.AddSingleton<UserNotificationService>();
            builder.Services.AddSingleton<TracksViewService>();
            //service.insights
            builder.Services.AddSingleton<ArtistInsightsViewService>();
            builder.Services.AddSingleton<CollectionStatisticsViewService>();
            builder.Services.AddSingleton<ReleaseInsightsViewService>();
            builder.Services.AddSingleton<TracksInsightsViewService>();


            return builder.Build();
        }
    }
}
