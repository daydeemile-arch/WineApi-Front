using System.Net.Http;
using System.Windows.Controls;

namespace SolutionClients.Views
{
    public partial class SuppliersView : Page  // ou UserControl
    {
        private readonly HttpClient _httpClient;

        public SuppliersView(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
        }
    }
}