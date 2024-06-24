using DiscogsInsight.DataAccess.Contract;
using DiscogsInsight.DataAccess.Models;
using DiscogsInsight.Database.Contract;
using DiscogsInsight.Database.Entities;
using Intents;

namespace DiscogsInsight.DataAccess.Services
{
    public class InsightsDataService : IInsightsDataService
    {
        private readonly ISQLiteAsyncConnection _db;

        public InsightsDataService(ISQLiteAsyncConnection db)
        {
            _db = db;
        }

        public Task<List<Release>> GetAllReleasesDataModelsAsList()
        {
            throw new NotImplementedException();
        }

        public async Task<TracksInsightDataModel> GetTrackInsightData()
        {
            //todo refactor this to return just the needed track data. use a query
            var tracks = await _db.Table<Track>().ToListAsync();


            var averageTrackLengthFormatted = GetAverageTrackLengthStringFormatted(tracks);
            var averageTracksPerReleaseText = GetAverageTracksPerReleaseStringFormatted(tracks);

            return new TracksInsightDataModel
            {
                AverageTrackLength = averageTrackLengthFormatted,//in progress dont use entire table to do this calculation
                AverageTracksPerRelease = averageTracksPerReleaseText//see above
            };
        }
        
        //taken from tracksinsightviewservice and now returning just a model and doign the conversions in view layer

        private static string GetAverageTracksPerReleaseStringFormatted(List<Track> tracks)
        {
            var releasesToTracks = tracks.GroupBy(x => x.DiscogsReleaseId).ToList();

            var tracksPerReleaseCount = new List<int>();
            foreach (var release in releasesToTracks)
            {
                tracksPerReleaseCount.Add(release.Count());
            }
            var averageTracksPerReleaseText = Math.Round(tracksPerReleaseCount.Average(), 0, MidpointRounding.AwayFromZero).ToString();
            return averageTracksPerReleaseText;
        }

        private static string GetAverageTrackLengthStringFormatted(List<Track> tracks)
        {
            var unformattedAverageTrackLength = tracks
                                                .Where(x => x.MusicBrainzTrackLength != null)
                                                .Average(x => x.MusicBrainzTrackLength);
            var averageTrackLengthFormatted = "";
            if (unformattedAverageTrackLength.HasValue)
            {
                averageTrackLengthFormatted = $"{TimeSpan.FromMilliseconds(unformattedAverageTrackLength.Value).ToString(@"mm\:ss")}";
            }

            return averageTrackLengthFormatted;
        }
    }
}
