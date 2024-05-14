using Microsoft.Extensions.Logging;
using DiscogsInsight.DataAccess;
using DiscogsInsight.DataAccess.Services;
using DiscogsInsight.ApiIntegration.Services;
using DiscogsInsight.View.Services.Tracks;
using DiscogsInsight.View.Services.Collection;
using DiscogsInsight.View.Services.Settings;
using DiscogsInsight.View.Services.Artist;
using DiscogsInsight.View.Services.Releases;
using DiscogsInsight.View.Services.Notifications;
using MudBlazor.Services;
using DiscogsInsight.View.Services.Insights;
using DiscogsInsight.DataAccess.Contract;
using SQLite;

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
    

            //Data layer
            builder.Services.AddSingleton<ISQLiteAsyncConnection, SQLiteAsyncConnectionAdapter>();
            builder.Services.AddSingleton<IDiscogsInsightDb, DiscogsInsightDb>();

            //Api layer
            builder.Services.AddSingleton<DiscogsApiService>();
            builder.Services.AddSingleton<MusicBrainzApiService>();
            builder.Services.AddSingleton<CoverArtArchiveApiService>();
            builder.Services.AddSingleton<LastFmApiService>();

            //rest of data layer
            builder.Services.AddSingleton<ICollectionDataService,CollectionDataService>();
            builder.Services.AddSingleton<IReleaseDataService,ReleaseDataService>();
            builder.Services.AddSingleton<SettingsDataService>();
            builder.Services.AddSingleton<ITagsDataService,TagsDataService>();
            builder.Services.AddSingleton<ITracksDataService,TracksDataService>();
            builder.Services.AddSingleton<IArtistDataService,ArtistDataService>();
            builder.Services.AddSingleton<IDiscogsGenresAndTagsDataService, DiscogsGenresAndTagsDataService>();

            //view layer
            builder.Services.AddSingleton<UserNotificationService>();
            builder.Services.AddSingleton<CollectionViewService>();
            builder.Services.AddSingleton<CollectionStatisticsViewService>();
            builder.Services.AddSingleton<SettingsViewService>();
            builder.Services.AddSingleton<ArtistViewService>();
            builder.Services.AddSingleton<TracksViewService>();
            builder.Services.AddSingleton<ReleaseViewService>();

            builder.Services.AddSingleton<ReleaseInsightsViewService>();
            builder.Services.AddSingleton<ArtistInsightsViewService>();
            builder.Services.AddSingleton<TracksInsightsViewService>();


            return builder.Build();
        }
    }
}
