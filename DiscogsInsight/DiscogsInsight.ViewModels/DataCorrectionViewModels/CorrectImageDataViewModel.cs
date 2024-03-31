using System.Diagnostics.Contracts;

namespace DiscogsInsight.ViewModels.DataCorrectionViewModels
{
    public class CorrectReleaseDataViewModel
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string MusicBrainzReleaseId { get; set; }
    }
}
