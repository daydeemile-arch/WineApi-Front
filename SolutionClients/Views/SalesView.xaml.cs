using Newtonsoft.Json;
using SolutionClients.Models;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace SolutionClients.Views
{
    public partial class SalesView : Page
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5120";
        private List<Sale> _allSales = new();
        private int? _selectedSaleId = null;

        public SalesView(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
            Loaded += async (s, e) =>
            {
                await LoadSales();
                await LoadCustomers();
                await LoadWines();
            };
        }

        // ─────────────────────────────────────────
        // CHARGEMENT
        // ─────────────────────────────────────────

        private async Task LoadSales()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Sales");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _allSales = JsonConvert.DeserializeObject<List<Sale>>(result) ?? new();
                    dgSales.ItemsSource = null;
                    dgSales.ItemsSource = _allSales;
                }
                else
                {
                    MessageBox.Show($"Erreur : {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des ventes : {ex.Message}");
            }
        }

        private async Task LoadCustomers()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Customers");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var customers = JsonConvert.DeserializeObject<List<Customer>>(result) ?? new();
                    cbCustomer.ItemsSource = customers;
                    cbCustomer.DisplayMemberPath = "LastName";
                    cbCustomer.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement clients : {ex.Message}");
            }
        }

        private async Task LoadWines()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Wines");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var wines = JsonConvert.DeserializeObject<List<Wine>>(result) ?? new();
                    cbWine.ItemsSource = wines;
                    cbWine.DisplayMemberPath = "Name";
                    cbWine.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement vins : {ex.Message}");
            }
        }

        private async Task LoadSaleLines(Sale sale)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/SalesLine/{sale.Id}");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    sale.Lines = JsonConvert.DeserializeObject<List<SaleLine>>(result) ?? new();
                    dgSales.Items.Refresh();
                }
                // 404 = aucune ligne, on laisse Lines vide sans erreur
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement lignes : {ex.Message}");
            }
        }

        // ─────────────────────────────────────────
        // SÉLECTION
        // ─────────────────────────────────────────

        private async void dgSales_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSales.SelectedItem is Sale sale)
            {
                _selectedSaleId = sale.Id;

                // Remplir le formulaire
                cbCustomer.SelectedValue = sale.ClientId;
                txtTotal.Text = $"{sale.TotalAmount:F2}";
                dpSaleDate.SelectedDate = sale.DateSale;

                // Charger les lignes pour la sous-grille
                await LoadSaleLines(sale);
            }
        }

        // ─────────────────────────────────────────
        // RECHERCHE
        // ─────────────────────────────────────────

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearchSale.Text.ToLower();
            var filtered = _allSales
                .Where(s => s.CustomerName.ToLower().Contains(searchText) ||
                            s.Lines.Any(l => l.WineName.ToLower().Contains(searchText)))
                .ToList();

            dgSales.ItemsSource = filtered;
        }

        // ─────────────────────────────────────────
        // AJOUT
        // ─────────────────────────────────────────

        private async void BtnAddSale_Click(object sender, RoutedEventArgs e)
        {
            if (cbCustomer.SelectedValue == null || cbWine.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un client et un vin.");
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Veuillez entrer une quantité valide.");
                return;
            }

            try
            {
                var newSale = new
                {
                    IdClient = cbCustomer.SelectedValue,
                    Lignes = new[]
                    {
                        new
                        {
                            IdVin = cbWine.SelectedValue,
                            Quantite = quantity
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(newSale);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/api/Sales", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Vente ajoutée avec succès !");
                    ClearForm();
                    await LoadSales();
                }
                else
                {
                    MessageBox.Show($"Erreur : {result}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}");
            }
        }

        // ─────────────────────────────────────────
        // MODIFICATION
        // ─────────────────────────────────────────

        private async void BtnUpdateSale_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSaleId == null)
            {
                MessageBox.Show("Veuillez sélectionner une vente à modifier.");
                return;
            }

            if (cbCustomer.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un client.");
                return;
            }

            if (!decimal.TryParse(txtTotal.Text, out decimal totalAmount) || totalAmount <= 0)
            {
                MessageBox.Show("Veuillez entrer un montant valide.");
                return;
            }

            try
            {
                var updatedSale = new
                {
                    IdClient = cbCustomer.SelectedValue,
                    TotalAmount = totalAmount,
                    DateSale = dpSaleDate.SelectedDate ?? DateTime.Now
                };

                var json = JsonConvert.SerializeObject(updatedSale);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{BaseUrl}/api/Sales/{_selectedSaleId}", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Vente modifiée avec succès !");
                    ClearForm();
                    await LoadSales();
                }
                else
                {
                    MessageBox.Show($"Erreur : {result}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}");
            }
        }

        // ─────────────────────────────────────────
        // SUPPRESSION
        // ─────────────────────────────────────────

        private async void BtnDeleteSale_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSaleId == null)
            {
                MessageBox.Show("Veuillez sélectionner une vente à supprimer.");
                return;
            }

            var confirmResult = MessageBox.Show(
                "Êtes-vous sûr de vouloir supprimer cette vente ?",
                "Confirmation de suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult == MessageBoxResult.No)
                return;

            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/api/Sales/{_selectedSaleId}");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Vente supprimée avec succès !");
                    ClearForm();
                    await LoadSales();
                }
                else
                {
                    MessageBox.Show($"Erreur : {result}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}");
            }
        }

        // ─────────────────────────────────────────
        // RESET FORMULAIRE
        // ─────────────────────────────────────────

        private void ClearForm()
        {
            cbCustomer.SelectedIndex = -1;
            cbWine.SelectedIndex = -1;
            txtQuantity.Clear();
            txtTotal.Clear();
            dpSaleDate.SelectedDate = null;
            txtSearchSale.Clear();
            dgSales.SelectedItem = null;
            _selectedSaleId = null;
        }
    }
}