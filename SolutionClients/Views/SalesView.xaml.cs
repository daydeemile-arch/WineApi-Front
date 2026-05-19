using System.Net.Http;
using System.Windows.Controls;

namespace SolutionClients.Views
{
    public partial class SalesView : Page  // ou UserControl
    {
        private readonly HttpClient _httpClient;

        public SalesView(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
        }
    }
}