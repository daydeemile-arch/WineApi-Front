using System.Net.Http;
using System.Windows.Controls;

namespace SolutionClients.Views
{
    public partial class StockView : Page  // ou UserControl
    {
        private readonly HttpClient _httpClient;

        public StockView(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
        }
    }
}