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
    public partial class StockView : Page
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5120";

        private List<Wine> _tousLesVins = new();
        private Wine? _vinSelectionne = null;

        public StockView(HttpClient httpClient)
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
                tktWineName.Text = vin.Name;
                tktWinePrice.Text = vin.Price.ToString("F2");
                tktWineQuantity.Text = vin.QuantityStock.ToString();
                tktWineQuantityUpdate.Text = vin.QuantiteCommande.ToString();
                tktWineType.Text = vin.Type;
            }
        }


        // ── Ajouter du stock ──
        private async void BtnAddStock_Click(object sender, RoutedEventArgs e)
        {
            if (_vinSelectionne == null)
            {
                SetStatut("Sélectionne un vin d'abord", "Orange");
                return;
            }

            if (!FormulaireValide()) return;

            int quantite = int.Parse(tktWineQuantityUpdate.Text);

            var content = new StringContent(
                JsonConvert.SerializeObject(quantite),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync(
                $"{BaseUrl}/api/Wines/{_vinSelectionne.Id}/stock", content);

            if (response.IsSuccessStatusCode)
            {
                SetStatut($"+{quantite} unités ajoutées", "Green");
                BtnClear_Click(sender, e);
                await ChargerVins();
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                SetStatut($"Erreur : {response.StatusCode} - {err}", "Red");
            }
        }

        // ── Retirer du stock ──
        private async void BtnRemoveStock_Click(object sender, RoutedEventArgs e)
        {
            if (_vinSelectionne == null)
            {
                SetStatut("Sélectionne un vin d'abord", "Orange");
                return;
            }

            if (!FormulaireValide()) return;

            int quantite = int.Parse(tktWineQuantityUpdate.Text);

            // On envoie une valeur négative pour décrémenter
            var content = new StringContent(
                JsonConvert.SerializeObject(-quantite),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync(
                $"{BaseUrl}/api/Wines/{_vinSelectionne.Id}/stock", content);

            if (response.IsSuccessStatusCode)
            {
                SetStatut($"-{quantite} unités retirées", "Green");
                BtnClear_Click(sender, e);
                await ChargerVins();
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                // L'API retourne BadRequest si le stock devient négatif
                SetStatut($"Erreur : {response.StatusCode} - {err}", "Red");
            }
        }

        

        // ── Vider formulaire ──
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            _vinSelectionne = null;
            tktWineName.Text = "";
            tktWinePrice.Text = "";
            tktWineQuantity.Text = "";
            tktWineQuantityUpdate.Text = "";
            dgWines.SelectedItem = null;
            txtSearch.Text = "";
            TxtStatut.Text = "";
        }

        // ── Validation ──
        private bool FormulaireValide()
        {
            if (!int.TryParse(tktWineQuantityUpdate.Text, out int val) || val <= 0)
            {
                SetStatut("Entre une quantité valide (entier > 0)", "Orange");
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