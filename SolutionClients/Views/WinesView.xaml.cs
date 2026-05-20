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
using SolutionClients.Models;

namespace SolutionClients.Views
{
    public partial class WinesView : Page
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5000";

        private List<Wine> _tousLesVins = new();
        private Wine? _vinSelectionne = null;

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
                    dgWines.ItemsSource = null;
                    dgWines.ItemsSource = _tousLesVins;
                    AfficherAlertes();
                    SetStatut($"{_tousLesVins.Count} vins chargés", "Green");
                }
                else
                {
                    SetStatut($"Erreur : {response.StatusCode}", "Red");
                }
            }
            catch (Exception ex)
            {
                SetStatut($"{ex.Message}", "Red");
            }
        }

        // ── Alertes stock bas ──
        private void AfficherAlertes()
        {
            var vinsEnAlerte = _tousLesVins.Where(v => v.IsBelowThreshold).ToList();

            if (vinsEnAlerte.Any())
            {
                var noms = string.Join(", ", vinsEnAlerte
                    .Select(v => $"{v.Name} ({v.QuantityStock}/{v.Threshold})"));
                TxtAlerte.Text = $"⚠️ Stock bas pour : {noms}";
                BandeauAlerte.Visibility = Visibility.Visible;
            }
            else
            {
                BandeauAlerte.Visibility = Visibility.Collapsed;
            }
        }

        // ── Recherche locale en temps réel ──
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var terme = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(terme))
            {
                dgWines.ItemsSource = _tousLesVins;
                SetStatut($"{_tousLesVins.Count} vins chargés", "Green");
                return;
            }

            var resultats = _tousLesVins
                .Where(v => v.Name.ToLower().Contains(terme))
                .ToList();

            dgWines.ItemsSource = resultats;
            SetStatut(resultats.Count > 0 ? $"{resultats.Count} résultat(s)" : "Aucun résultat",
                      resultats.Count > 0 ? "Green" : "Orange");
        }

        // ── Recherche bouton ──
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var terme = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(terme))
            {
                dgWines.ItemsSource = _tousLesVins;
                SetStatut($"{_tousLesVins.Count} vins chargés", "Green");
                return;
            }

            var resultats = _tousLesVins
                .Where(v => v.Name.ToLower().Contains(terme))
                .ToList();

            dgWines.ItemsSource = resultats;
            SetStatut(resultats.Count > 0 ? $"{resultats.Count} résultat(s)" : "Aucun résultat",
                      resultats.Count > 0 ? "Green" : "Orange");
        }

        // ── Sélection → remplissage formulaire ──
        private void dgWines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgWines.SelectedItem is Wine vin)
            {
                _vinSelectionne = vin;
                txtWineName.Text = vin.Name;
                txtPrice.Text = vin.Price.ToString("F2");
                txtStock.Text = vin.QuantityStock.ToString();
                txtThreshold.Text = vin.Threshold.ToString();
                txtQuantiteCommande.Text = vin.QuantiteCommande.ToString();
                cbWineType.SelectedItem = cbWineType.Items
                    .Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == vin.Type);
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
                type = cbWineType.SelectedItem is ComboBoxItem si ? si.Content.ToString() : "",
                quantity = int.Parse(txtStock.Text),
                waystock = int.Parse(txtThreshold.Text),
                idSupplier = 1,
                quantiteCommande = int.Parse(txtQuantiteCommande.Text)
            };

            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/api/Wines", content);

            if (response.IsSuccessStatusCode)
            {
                SetStatut("Vin ajouté", "Green");
                BtnClear_Click(sender, e);
                await ChargerVins();
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                SetStatut($"Erreur ajout : {response.StatusCode} - {err}", "Red");
            }
        }

        // ── Modifier ──
        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_vinSelectionne == null)
            {
                SetStatut("Sélectionne un vin d'abord", "Orange");
                return;
            }

            if (!FormulaireValide()) return;

            var body = new
            {
                name = txtWineName.Text,
                price = decimal.Parse(txtPrice.Text),
                type = cbWineType.SelectedItem is ComboBoxItem si ? si.Content.ToString() : "",
                quantity = int.Parse(txtStock.Text),
                waystock = int.Parse(txtThreshold.Text),
                idSupplier = _vinSelectionne.ProviderId,
                quantiteCommande = int.Parse(txtQuantiteCommande.Text)
            };

            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{BaseUrl}/api/Wines/{_vinSelectionne.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                SetStatut("Vin modifié", "Green");
                BtnClear_Click(sender, e);
                await ChargerVins();
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                SetStatut($"Erreur modification : {response.StatusCode} - {err}", "Red");
            }
        }

        // ── Supprimer ──
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_vinSelectionne == null)
            {
                SetStatut("Sélectionne un vin d'abord", "Orange");
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
                SetStatut("Vin supprimé", "Green");
                BtnClear_Click(sender, e);
                await ChargerVins();
            }
            else
            {
                SetStatut($"Erreur suppression : {response.StatusCode}", "Red");
            }
        }

        // ── Vider formulaire ──
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            _vinSelectionne = null;
            txtWineName.Text = "";
            txtPrice.Text = "";
            txtStock.Text = "";
            txtThreshold.Text = "";
            txtQuantiteCommande.Text = "";
            cbWineType.SelectedIndex = -1;
            dgWines.SelectedItem = null;
            txtSearch.Text = "";
            TxtStatut.Text = "";
        }

        // ── Validation ──
        private bool FormulaireValide()
        {
            if (string.IsNullOrWhiteSpace(txtWineName.Text) ||
                !decimal.TryParse(txtPrice.Text, out _) ||
                !int.TryParse(txtStock.Text, out _) ||
                !int.TryParse(txtThreshold.Text, out _) ||
                !int.TryParse(txtQuantiteCommande.Text, out _))
            {
                SetStatut("Remplis tous les champs correctement", "Orange");
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