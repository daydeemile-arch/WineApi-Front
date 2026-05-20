using Newtonsoft.Json;
using SolutionClients.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SolutionClients.Models;

namespace SolutionClients.Views
{
    public partial class WinesView : Page
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5000";

        private List<Wine> _tousLesVins = new();
        private Wine? _vinSelectionne = null;

        public WinesView(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
            Loaded += async (s, e) => await ChargerVins();
        }

        // ── Charger tous les vins ──
        private async Task ChargerVins()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Wines");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _tousLesVins = JsonConvert.DeserializeObject<List<Wine>>(result) ?? new();
                    dgWines.ItemsSource = _tousLesVins;
                    SetStatut($"✅ {_tousLesVins.Count} vins chargés", "Green");
                }
                else
                {
                    SetStatut($"❌ Erreur : {response.StatusCode}", "Red");
                }
            }
            catch (Exception ex)
            {
                SetStatut($"❌ {ex.Message}", "Red");
            }
        }

        // ── Recherche par nom via l'API ──
        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var nom = txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(nom))
            {
                await ChargerVins();
                return;
            }

            try
            {
                SetStatut("Recherche en cours...", "Gray");

                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Wines/byname/{Uri.EscapeDataString(nom)}");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // L'API retourne un objet unique → on le met dans une liste
                    var vin = JsonConvert.DeserializeObject<Wine>(result);
                    dgWines.ItemsSource = vin != null ? new List<Wine> { vin } : new List<Wine>();
                    SetStatut(vin != null ? "✅ 1 résultat" : "⚠️ Aucun résultat", vin != null ? "Green" : "Orange");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    dgWines.ItemsSource = null;
                    SetStatut("⚠️ Aucun vin trouvé", "Orange");
                }
                else
                {
                    SetStatut($"❌ Erreur : {response.StatusCode}", "Red");
                }
            }
            catch (Exception ex)
            {
                SetStatut($"❌ {ex.Message}", "Red");
            }
        }

        // ── Recherche déclenchée aussi sur Entrée ──
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Si le champ est vidé → recharge tout automatiquement
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                dgWines.ItemsSource = _tousLesVins;
                SetStatut($"✅ {_tousLesVins.Count} vins chargés", "Green");
            }
        }

        // ── Sélection → remplissage du formulaire ──
        private void dgWines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgWines.SelectedItem is Wine vin)
            {
                _vinSelectionne = vin;
                txtWineName.Text = vin.Name;
                txtPrice.Text = vin.Price.ToString("F2");
                txtStock.Text = vin.QuantityStock.ToString();
                txtThreshold.Text = vin.Threshold.ToString();
            }
        }

        // ── Ajouter ──
        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!FormulaireValide()) return;

            var body = new
            {
                name = txtWineName.Text,
                price = decimal.Parse(txtPrice.Text),
                quantityStock = int.Parse(txtStock.Text),
                threshold = int.Parse(txtThreshold.Text)
            };

            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/api/Wines", content);

            if (response.IsSuccessStatusCode)
            {
                SetStatut("✅ Vin ajouté", "Green");
                BtnClear_Click(sender, e);
                await ChargerVins();
            }
            else
            {
                SetStatut($"❌ Erreur ajout : {response.StatusCode}", "Red");
            }
        }

        // ── Modifier ──
        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_vinSelectionne == null)
            {
                SetStatut("⚠️ Sélectionne un vin d'abord", "Orange");
                return;
            }

            if (!FormulaireValide()) return;

            var body = new
            {
                id = _vinSelectionne.Id,
                name = txtWineName.Text,
                price = decimal.Parse(txtPrice.Text),
                quantityStock = int.Parse(txtStock.Text),
                threshold = int.Parse(txtThreshold.Text)
            };

            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/api/Wines/{_vinSelectionne.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                SetStatut("✅ Vin modifié", "Green");
                BtnClear_Click(sender, e);
                await ChargerVins();
            }
            else
            {
                SetStatut($"❌ Erreur modification : {response.StatusCode}", "Red");
            }
        }

        // ── Supprimer ──
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_vinSelectionne == null)
            {
                SetStatut("⚠️ Sélectionne un vin d'abord", "Orange");
                return;
            }

            var confirm = MessageBox.Show(
                $"Supprimer « {_vinSelectionne.Name} » ?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            var response = await _httpClient.DeleteAsync($"{BaseUrl}/api/Wines/{_vinSelectionne.Id}");

            if (response.IsSuccessStatusCode)
            {
                SetStatut("✅ Vin supprimé", "Green");
                BtnClear_Click(sender, e);
                await ChargerVins();
            }
            else
            {
                SetStatut($"❌ Erreur suppression : {response.StatusCode}", "Red");
            }
        }

        // ── Vider le formulaire ──
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            _vinSelectionne = null;
            txtWineName.Text = "";
            txtPrice.Text = "";
            txtStock.Text = "";
            txtThreshold.Text = "";
            cbWineType.SelectedIndex = -1;
            dgWines.SelectedItem = null;
            txtSearch.Text = "";
            TxtStatut.Text = "";
        }

        // ── Helpers ──
        private bool FormulaireValide()
        {
            if (string.IsNullOrWhiteSpace(txtWineName.Text) ||
                !decimal.TryParse(txtPrice.Text, out _) ||
                !int.TryParse(txtStock.Text, out _) ||
                !int.TryParse(txtThreshold.Text, out _))
            {
                SetStatut("⚠️ Remplis tous les champs correctement", "Orange");
                return false;
            }
            return true;
        }

        private void SetStatut(string message, string couleur)
        {
            TxtStatut.Text = message;
            TxtStatut.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(couleur));
        }
    }
}