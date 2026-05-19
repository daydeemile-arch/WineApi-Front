using System.Net.Http;
using System.Windows.Controls;

namespace SolutionClients.Views
{
    public partial class CustomersView : Page  // ou UserControl
    {
        private readonly HttpClient _httpClient;

        public CustomersView(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
        }
    }
}