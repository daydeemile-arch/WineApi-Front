using Newtonsoft.Json;
using SolutionClients.Models;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SolutionClients.Views
{
    public partial class CartView : Page
    {
        private readonly HttpClient _httpClient;
        private readonly List<Wine> _cart;

        private const string BaseUrl =
            "http://localhost:5120";

        public CartView(
            HttpClient httpClient,
            List<Wine> cart)
        {
            InitializeComponent();

            _httpClient = httpClient;
            _cart = cart;

            dgCart.ItemsSource = null;
            dgCart.ItemsSource = _cart;
        }

        private async void BtnCheckout_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (!_cart.Any())
                {
                    MessageBox.Show(
                        "Cart is empty!");
                    return;
                }

                var request = new
                {
                    customerId = 1, // temporaire
                    items = _cart.Select(w =>
                        new
                        {
                            wineId = w.Id,
                            quantity = 1
                        })
                };

                var json =
                    JsonConvert.SerializeObject(
                        request);

                var content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json");

                var response =
                    await _httpClient.PostAsync(
                        $"{BaseUrl}/api/Sales/checkout",
                        content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "✅ Order placed successfully!");

                    _cart.Clear();

                    dgCart.ItemsSource = null;
                }
                else
                {
                    var error =
                        await response.Content
                            .ReadAsStringAsync();

                    MessageBox.Show(
                        $"❌ Error: {error}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ {ex.Message}");
            }
        }
    }
}