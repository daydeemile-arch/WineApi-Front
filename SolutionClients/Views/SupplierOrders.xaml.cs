using Newtonsoft.Json;
using SolutionClients.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SolutionClients.Views
{
    public partial class SupplierOrders : Page
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5120";
        private List<CommandeFournisseur> _toutesLesCommandes = new();

        public SupplierOrders(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
            Loaded += async (s, e) =>
            {
                try
                {
                    await ChargerCommandes();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur au chargement :\n\n{ex.GetType().Name}\n{ex.Message}\n\n{ex.StackTrace}",
                                    "Crash StockView", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }
        private async Task ChargerCommandes()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/SupplierOrders");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _toutesLesCommandes = JsonConvert.DeserializeObject<List<CommandeFournisseur>>(result) ?? new();
                    AppliquerFiltre();
                }
                else
                {
                    MessageBox.Show($"Erreur chargement : {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}");
            }
        }

        private void AppliquerFiltre()
        {
            // dgCommandes peut être null si appelé avant la fin d'InitializeComponent
            if (dgCommandes == null) return;

            var filtre = (cbFiltreStatut.SelectedItem as ComboBoxItem)?.Content.ToString();

            var resultats = filtre == "Tous"
                ? _toutesLesCommandes
                : _toutesLesCommandes.Where(c => c.StatutCommande == filtre).ToList();

            dgCommandes.ItemsSource = null;
            dgCommandes.ItemsSource = resultats;
        }

        private void cbFiltreStatut_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => AppliquerFiltre();

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
            => await ChargerCommandes();

        private async void BtnAccepter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int idCommande))
            {
                var confirm = MessageBox.Show(
                    "Confirmer l'acceptation de cette commande ?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes) return;

                var response = await _httpClient.PatchAsync(
                    $"{BaseUrl}/api/SupplierOrders/{idCommande}/accepter",
                    new StringContent("", Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                    await ChargerCommandes();
                else
                    MessageBox.Show($"Erreur : {response.StatusCode}");
            }
        }
    }
}