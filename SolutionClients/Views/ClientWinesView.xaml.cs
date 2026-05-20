using Newtonsoft.Json;
using SolutionClients.Models;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace SolutionClients.Views
{
    public partial class ClientWinesView : Page
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5120";

        private readonly List<Wine> _cart;

        public ClientWinesView(HttpClient httpClient,
                               List<Wine> cart)
        {
            InitializeComponent();

            _httpClient = httpClient;
            _cart = cart;

            Loaded += async (s, e) => await ChargerVins();
        }
        private async Task ChargerVins()
        {
            try
            {
                var response =
                    await _httpClient.GetAsync($"{BaseUrl}/api/Wines");

                var result =
                    await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var vins =
                        JsonConvert.DeserializeObject<List<Wine>>(result);

                    dgWines.ItemsSource = vins;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnBuy_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Wine wine)
            {
                _cart.Add(wine);

                MessageBox.Show(
                    $"🛒 {wine.Name} added to cart!");
            }
        }
    }
}