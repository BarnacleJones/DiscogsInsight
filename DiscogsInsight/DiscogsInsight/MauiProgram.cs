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

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<HttpClient>();
            //Data layer
            builder.Services.AddSingleton<DiscogsInsightDb>();
            //Api layer
            builder.Services.AddSingleton<DiscogsApiService>();

            //rest of data layer - does the order matter?
            builder.Services.AddSingleton<CollectionDataService>();
            builder.Services.AddSingleton<SettingsDataService>();
            builder.Services.AddSingleton<ArtistDataService>();
            builder.Services.AddSingleton<TracksViewService>();
            builder.Services.AddSingleton<ReleaseDataService>();

            //view layer
            builder.Services.AddSingleton<UserNotificationService>();
            builder.Services.AddSingleton<CollectionViewService>();
            builder.Services.AddSingleton<SettingsViewService>();
            builder.Services.AddSingleton<ArtistViewService>();
            builder.Services.AddSingleton<TracksViewService>();
            builder.Services.AddSingleton<ReleaseViewService>();


            return builder.Build();
        }
    }
}
