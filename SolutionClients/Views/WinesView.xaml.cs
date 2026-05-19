using Newtonsoft.Json;
using SolutionClients.Models;
using System.Buffers.Text;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using SolutionClients.Models;

namespace SolutionClients.Views
{
    public partial class WinesView : Page  // ou UserControl
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5120";

        public WinesView(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
            Loaded += async (s, e) => await ChargerVins();
        }
        private async Task ChargerVins()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Wines");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var vins = JsonConvert.DeserializeObject<List<Wine>>(result);
                    dgWines.ItemsSource = vins;
                }
                else
                {
                    MessageBox.Show($"Erreur : {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}");
            }
        }
        private void dgWines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgWines.SelectedItem is Wine vinSelectionne)
            {
                // Option A — Afficher dans des champs texte de la même page
                txtWineName.Text = vinSelectionne.Name;
                txtPrice.Text = $"{vinSelectionne.Price} €";
                txtStock.Text = vinSelectionne.QuantityStock.ToString();
                txtThreshold.Text = vinSelectionne.Threshold.ToString();

                // Option B — Ouvrir une fenêtre de détail
                // var detail = new VinDetailWindow(vinSelectionne);
                // detail.ShowDialog();
            }
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            }
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {

        }
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {

        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {

        }
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {

        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {

        }


    }

}