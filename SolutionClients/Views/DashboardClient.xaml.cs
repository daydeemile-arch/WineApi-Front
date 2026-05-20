using SolutionClients.Models;
using SolutionClients.Views;
using System.Net.Http;
using System.Windows;

namespace SolutionClients
{
    public partial class DashboardClient : Window
    {
        private readonly HttpClient _httpClient =
            new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5120")
            };
     private List<Wine> _cart = new();

        public DashboardClient()
        {
            InitializeComponent();

            MainFrame.Content = new ClientWinesView(_httpClient, _cart);
        }

        private void BtnWines_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content =
               new ClientWinesView(_httpClient, _cart);
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content =
     new CartView(
         _httpClient,
         _cart);
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content =
                new MyProfileView(_httpClient);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();

            this.Close();
        }
    }
}