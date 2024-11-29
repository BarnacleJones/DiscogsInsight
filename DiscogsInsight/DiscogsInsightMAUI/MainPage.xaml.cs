using System.Collections.ObjectModel;

namespace DiscogsInsightMAUI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
    }

    public class Model
    {
        public DateTime Date { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
    }

    public class ViewModel
    {
        public ObservableCollection<Model> Temperature { get; set; }

        public ViewModel()
        {
            Temperature = new ObservableCollection<Model>
            {
                new Model { Date = new DateTime(2024,11,5),  MinValue = 16, MaxValue = 20 },
                new Model { Date = new DateTime(2024,11,10), MinValue = 20, MaxValue = 25 },
                new Model { Date = new DateTime(2024,11,15), MinValue = 11,  MaxValue = 15 },
                new Model { Date = new DateTime(2024,11,20), MinValue = 15,  MaxValue = 20 },
                new Model { Date = new DateTime(2024,11,25), MinValue = 10,  MaxValue = 15 },
                new Model { Date = new DateTime(2024,11,30), MinValue = 21,  MaxValue = 25 }
            };
        }
    }
}