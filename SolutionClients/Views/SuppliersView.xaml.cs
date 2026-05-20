using Newtonsoft.Json;
using SolutionClients.Models;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace SolutionClients.Views
{
    public partial class SuppliersView : Page
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5120";
        private List<Supplier> _allSuppliers = new();
        private int? _selectedSupplierId = null;
        private int? _currentAddressId = null;

        public SuppliersView(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
            Loaded += async (s, e) => await LoadSuppliers();
        }

        private async Task LoadSuppliers()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Suppliers");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _allSuppliers = JsonConvert.DeserializeObject<List<Supplier>>(result) ?? new();

                    // Charger toutes les adresses avant d'assigner au DataGrid
                    var tasks = _allSuppliers.Select(s => FetchAndSetAddressForSupplier(s)).ToList();
                    await Task.WhenAll(tasks);

                    // Assigner seulement après que toutes les adresses sont chargées
                    dgSuppliers.ItemsSource = null;
                    dgSuppliers.ItemsSource = _allSuppliers.ToList(); // ToList() force une nouvelle référence
                }
                else
                {
                    MessageBox.Show($"Erreur API Suppliers: {response.StatusCode}\n{result}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement : {ex.Message}");
            }
        }

        private async void dgSuppliers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSuppliers.SelectedItem is Supplier supplierSelectionne)
            {
                _selectedSupplierId = supplierSelectionne.Id;
                _currentAddressId = supplierSelectionne.IdAdress > 0 ? supplierSelectionne.IdAdress : (int?)null;

                txtSupplierName.Text = supplierSelectionne.Name;
                txtPhone.Text = supplierSelectionne.PhoneNumber;
                txtEmail.Text = supplierSelectionne.Email;

                await FetchAndSetAddress(supplierSelectionne);
            }
        }

        private async Task FetchAndSetAddressForSupplier(Supplier supplier)
        {
            try
            {
                if (supplier.IdAdress <= 0)
                {
                    supplier.FullAddress = "Adresse non définie";
                    return;
                }

                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Address/{supplier.IdAdress}");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var address = JsonConvert.DeserializeObject<Address>(result);
                    supplier.FullAddress = address?.FullAddress ?? "Adresse non définie";
                }
                else
                {
                    supplier.FullAddress = "Adresse non définie";
                }
            }
            catch
            {
                supplier.FullAddress = "Adresse non définie";
            }
        }

   

        private async Task FetchAndSetAddress(Supplier supplier)
        {
            try
            {
                if (supplier.IdAdress <= 0) return;

                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Address/{supplier.IdAdress}");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var address = JsonConvert.DeserializeObject<Address>(result);
                    if (address != null)
                    {
                        // ← noms alignés sur Address.cs corrigé
                        txtNumRue.Text = address.StreetNumber;
                        txtNomRue.Text = address.StreetName;
                        txtCodePostal.Text = address.PostalCode;
                        txtVille.Text = address.City;
                    }
                }
                else
                {
                    MessageBox.Show($"Erreur API Address/{supplier.IdAdress}: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement adresse : {ex.Message}");
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearchSupplier.Text.ToLower();
            var filtered = _allSuppliers
                .Where(s => s.Name.ToLower().Contains(searchText) ||
                            s.Email.ToLower().Contains(searchText) ||
                            s.PhoneNumber.ToLower().Contains(searchText))
                .ToList();
            dgSuppliers.ItemsSource = filtered;
        }

        private async void BtnAddSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
            {
                MessageBox.Show("Veuillez remplir tous les champs.");
                return;
            }

            try
            {
                // 1. Créer l'adresse — noms alignés sur AdressCreateDto
                var newAddress = new
                {
                    StreetNumber = txtNumRue.Text,
                    StreetName = txtNomRue.Text,
                    PostalCode = txtCodePostal.Text,
                    City = txtVille.Text
                };

                var jsonAddress = JsonConvert.SerializeObject(newAddress);
                var contentAddress = new StringContent(jsonAddress, System.Text.Encoding.UTF8, "application/json");
                var responseAddress = await _httpClient.PostAsync($"{BaseUrl}/api/Address", contentAddress);

                if (!responseAddress.IsSuccessStatusCode)
                {
                    string errorDetail = await responseAddress.Content.ReadAsStringAsync();
                    MessageBox.Show($"Erreur création adresse.\nCode : {responseAddress.StatusCode}\n{errorDetail}");
                    return;
                }

                var addressResult = await responseAddress.Content.ReadAsStringAsync();
                var addressData = JsonConvert.DeserializeObject<Address>(addressResult);

                if (addressData == null || addressData.Id == 0)
                {
                    MessageBox.Show("Erreur : impossible de récupérer l'ID de l'adresse créée.");
                    return;
                }

                // 2. Créer le fournisseur — IdAdresse aligné sur SupplierCreateDto
                var newSupplier = new
                {
                    SupplierName = txtSupplierName.Text,
                    PhoneNumber = txtPhone.Text,
                    Email = txtEmail.Text,
                    IdAdresse = addressData.Id  // ← corrigé
                };

                var jsonSupplier = JsonConvert.SerializeObject(newSupplier);
                var contentSupplier = new StringContent(jsonSupplier, System.Text.Encoding.UTF8, "application/json");
                var responseSupplier = await _httpClient.PostAsync($"{BaseUrl}/api/Suppliers", contentSupplier);

                if (responseSupplier.IsSuccessStatusCode)
                {
                    MessageBox.Show("Fournisseur et adresse ajoutés avec succès !");
                    ClearForm();
                    await LoadSuppliers();
                }
                else
                {
                    var err = await responseSupplier.Content.ReadAsStringAsync();
                    MessageBox.Show($"Erreur création fournisseur : {err}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}");
            }
        }

        private async void BtnUpdateSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSupplierId == null)
            {
                MessageBox.Show("Veuillez sélectionner un fournisseur à modifier.");
                return;
            }

            if (!ValidateForm())
            {
                MessageBox.Show("Veuillez remplir tous les champs.");
                return;
            }

            try
            {
                // 1. Mettre à jour l'adresse — noms alignés sur AdressCreateDto
                if (_currentAddressId.HasValue)
                {
                    var updatedAddress = new
                    {
                        StreetNumber = txtNumRue.Text,
                        StreetName = txtNomRue.Text,
                        PostalCode = txtCodePostal.Text,
                        City = txtVille.Text
                    };

                    var jsonAddress = JsonConvert.SerializeObject(updatedAddress);
                    var contentAddress = new StringContent(jsonAddress, System.Text.Encoding.UTF8, "application/json");
                    var responseAddress = await _httpClient.PutAsync($"{BaseUrl}/api/Address/{_currentAddressId}", contentAddress);

                    if (!responseAddress.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Erreur mise à jour adresse : {responseAddress.StatusCode}");
                        return;
                    }
                }

                // 2. Mettre à jour le fournisseur — IdAdresse aligné sur SupplierCreateDto
                var updatedSupplier = new
                {
                    Id = _selectedSupplierId,
                    SupplierName = txtSupplierName.Text,
                    PhoneNumber = txtPhone.Text,
                    Email = txtEmail.Text,
                    IdAdresse = _currentAddressId ?? 1  // ← corrigé
                };

                var jsonSupplier = JsonConvert.SerializeObject(updatedSupplier);
                var contentSupplier = new StringContent(jsonSupplier, System.Text.Encoding.UTF8, "application/json");
                var responseSupplier = await _httpClient.PutAsync($"{BaseUrl}/api/Suppliers/{_selectedSupplierId}", contentSupplier);

                if (responseSupplier.IsSuccessStatusCode)
                {
                    MessageBox.Show("Fournisseur et adresse modifiés avec succès !");
                    ClearForm();
                    await LoadSuppliers();
                    _selectedSupplierId = null;
                }
                else
                {
                    var err = await responseSupplier.Content.ReadAsStringAsync();
                    MessageBox.Show($"Erreur mise à jour fournisseur : {err}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}");
            }
        }

        private async void BtnDeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSupplierId == null)
            {
                MessageBox.Show("Veuillez sélectionner un fournisseur à supprimer.");
                return;
            }

            var result = MessageBox.Show(
                "Êtes-vous sûr de vouloir supprimer ce fournisseur ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No) return;

            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/api/Suppliers/{_selectedSupplierId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Fournisseur supprimé avec succès !");
                    ClearForm();
                    await LoadSuppliers();
                }
                else
                {
                    MessageBox.Show($"Erreur : {responseContent}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}");
            }
        }

        private bool ValidateForm()
        {
            return !string.IsNullOrWhiteSpace(txtSupplierName.Text) &&
                   !string.IsNullOrWhiteSpace(txtPhone.Text) &&
                   !string.IsNullOrWhiteSpace(txtEmail.Text) &&
                   !string.IsNullOrWhiteSpace(txtNumRue.Text) &&
                   !string.IsNullOrWhiteSpace(txtNomRue.Text) &&
                   !string.IsNullOrWhiteSpace(txtCodePostal.Text) &&
                   !string.IsNullOrWhiteSpace(txtVille.Text);
        }

        private void ClearForm()
        {
            txtSupplierName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtNumRue.Clear();
            txtNomRue.Clear();
            txtCodePostal.Clear();
            txtVille.Clear();
            txtSearchSupplier.Clear();
            dgSuppliers.SelectedItem = null;
            _selectedSupplierId = null;
            _currentAddressId = null;
        }
    }
}