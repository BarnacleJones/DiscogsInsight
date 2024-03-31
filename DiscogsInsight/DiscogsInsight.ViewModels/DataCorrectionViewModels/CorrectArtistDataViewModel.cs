namespace DiscogsInsight.ViewModels.DataCorrectionViewModels
{
    public class CorrectArtistDataViewModel
    {
        public string CorrectArtistMusicBrainzId { get; set; }
        public string ArtistName { get; set; }
        public string Tags { get; set; }

        public override string ToString()
        {
            return $"{ArtistName} - {Tags}";
        }
    }
}
