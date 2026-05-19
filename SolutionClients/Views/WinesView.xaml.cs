using System.Net.Http;
using System.Windows.Controls;

namespace SolutionClients.Views
{
    public partial class WinesView : Page  // ou UserControl
    {
        private readonly HttpClient _httpClient;

        public WinesView(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
        }
        private void dgWines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // logique à compléter
        }
    }

}