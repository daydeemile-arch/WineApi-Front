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
        private const string BaseUrl = "http://localhost:5000";
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

                    // Récupérer l'adresse complète pour chaque fournisseur
                    foreach (var supplier in _allSuppliers)
                    {
                        await FetchAndSetAddressForSupplier(supplier);
                    }

                    dgSuppliers.ItemsSource = _allSuppliers;
                }
                else
                {
                    MessageBox.Show($"Erreur : {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement : {ex.Message}");
            }
        }

        private async Task FetchAndSetAddressForSupplier(Supplier supplier)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Address/{supplier.IdAdress}");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var address = JsonConvert.DeserializeObject<Address>(result);
                    if (address != null)
                    {
                        supplier.FullAddress = address.FullAddress;
                    }
                }
                else
                {
                    supplier.FullAddress = $"ID Adresse: {supplier.IdAdress}";
                }
            }
            catch
            {
                supplier.FullAddress = $"ID Adresse: {supplier.IdAdress}";
            }
        }

        private async void dgSuppliers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSuppliers.SelectedItem is Supplier supplierSelectionnee)
            {
                _selectedSupplierId = supplierSelectionnee.Id;
                await LoadSupplierDetails(supplierSelectionnee.Id);
            }
        }

        private async Task LoadSupplierDetails(int supplierId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Suppliers/{supplierId}");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var supplier = JsonConvert.DeserializeObject<Supplier>(result);
                    if (supplier != null)
                    {
                        _currentAddressId = supplier.IdAdress;
                        
                        // Récupérer l'adresse complète
                        await FetchAndSetAddress(supplier);
                        
                        // Remplir les champs du formulaire
                        txtSupplierName.Text = supplier.Name;
                        txtPhone.Text = supplier.PhoneNumber;
                        txtEmail.Text = supplier.Email;
                    }
                }
                else
                {
                    MessageBox.Show($"Erreur : {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des détails : {ex.Message}");
            }
        }

        private async Task FetchAndSetAddress(Supplier supplier)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Address/{supplier.IdAdress}");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var address = JsonConvert.DeserializeObject<Address>(result);
                    if (address != null)
                    {
                        txtNumRue.Text = address.NumRue;
                        txtNomRue.Text = address.NomRue;
                        txtCodePostal.Text = address.CodePostal;
                        txtVille.Text = address.Ville;
                        supplier.FullAddress = address.FullAddress;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement de l'adresse : {ex.Message}");
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
                // 1. Créer l'adresse en premier
                var newAddress = new
                {
                    NumRue = txtNumRue.Text,
                    NomRue = txtNomRue.Text,
                    CodePostal = txtCodePostal.Text,
                    Ville = txtVille.Text
                };

                var jsonAddress = JsonConvert.SerializeObject(newAddress);
                var contentAddress = new StringContent(jsonAddress, System.Text.Encoding.UTF8, "application/json");

                var responseAddress = await _httpClient.PostAsync($"{BaseUrl}/api/Address", contentAddress);
                
                if (!responseAddress.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Erreur lors de la création de l'adresse : {responseAddress.StatusCode}");
                    return;
                }

                var addressResult = await responseAddress.Content.ReadAsStringAsync();
                var addressData = JsonConvert.DeserializeObject<Address>(addressResult);
                
                if (addressData == null)
                {
                    MessageBox.Show("Erreur : impossible de récupérer l'ID de l'adresse créée.");
                    return;
                }

                // 2. Créer le fournisseur avec l'ID de l'adresse
                var newSupplier = new
                {
                    SupplierName = txtSupplierName.Text,
                    PhoneNumber = txtPhone.Text,
                    Email = txtEmail.Text,
                    IdAddress = addressData.Id
                };

                var jsonSupplier = JsonConvert.SerializeObject(newSupplier);
                var contentSupplier = new StringContent(jsonSupplier, System.Text.Encoding.UTF8, "application/json");

                var responseSupplier = await _httpClient.PostAsync($"{BaseUrl}/api/Suppliers", contentSupplier);
                var supplierResult = await responseSupplier.Content.ReadAsStringAsync();

                if (responseSupplier.IsSuccessStatusCode)
                {
                    MessageBox.Show("Fournisseur et adresse ajoutés avec succès !");
                    ClearForm();
                    await LoadSuppliers();
                }
                else
                {
                    MessageBox.Show($"Erreur : {supplierResult}");
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
                // 1. Mettre à jour l'adresse
                if (_currentAddressId.HasValue)
                {
                    var updatedAddress = new
                    {
                        IdAdresses = _currentAddressId,
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
                        MessageBox.Show($"Erreur lors de la mise à jour de l'adresse : {responseAddress.StatusCode}");
                        return;
                    }
                }

                // 2. Mettre à jour le fournisseur
                var updatedSupplier = new
                {
                    Id = _selectedSupplierId,
                    SupplierName = txtSupplierName.Text,
                    PhoneNumber = txtPhone.Text,
                    Email = txtEmail.Text,
                    IdAddress = _currentAddressId ?? 1
                };

                var jsonSupplier = JsonConvert.SerializeObject(updatedSupplier);
                var contentSupplier = new StringContent(jsonSupplier, System.Text.Encoding.UTF8, "application/json");

                var responseSupplier = await _httpClient.PutAsync($"{BaseUrl}/api/Suppliers/{_selectedSupplierId}", contentSupplier);
                var supplierResult = await responseSupplier.Content.ReadAsStringAsync();

                if (responseSupplier.IsSuccessStatusCode)
                {
                    MessageBox.Show("Fournisseur et adresse modifiés avec succès !");
                    ClearForm();
                    await LoadSuppliers();
                    _selectedSupplierId = null;
                }
                else
                {
                    MessageBox.Show($"Erreur : {supplierResult}");
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
                $"Êtes-vous sûr de vouloir supprimer ce fournisseur ?",
                "Confirmation de suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/api/Suppliers/{_selectedSupplierId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Fournisseur supprimé avec succès !");
                    ClearForm();
                    await LoadSuppliers();
                    _selectedSupplierId = null;
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
