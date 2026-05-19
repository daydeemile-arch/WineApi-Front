using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;

namespace SolutionClients.Views
{
    public partial class Dashboard : Window
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;

        public Dashboard(HttpClient httpClient, string token)
        {
            InitializeComponent();
            _httpClient = httpClient;
            _token = token;

            // S'assure que le token est bien dans les headers
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            // Ouvre Wines par défaut au démarrage
            MainFrame.Content = new WinesView(_httpClient);
        }

        private void BtnWines_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new WinesView(_httpClient);
        }

        private void BtnCustomers_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new CustomersView(_httpClient);
        }

        private void BtnSuppliers_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new SuppliersView(_httpClient);
        }

        private void BtnSales_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new SalesView(_httpClient);
        }

        private void BtnStock_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new StockView(_httpClient);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();

            this.Close();
        }
    }
}