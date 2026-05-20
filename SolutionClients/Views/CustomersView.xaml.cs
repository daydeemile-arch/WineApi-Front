using Newtonsoft.Json;
using SolutionClients.Models;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SolutionClients.Views
{
    public partial class CustomersView : Page
    {
        private List<Customer> _tousLesClients = new();
        private Customer? _clientSelectionne = null;
        private int? _currentAddressId = null;

        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5120";

        public CustomersView(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
            Loaded += async (s, e) => await ChargerClients();
        }

        // ── Charger tous les clients ──
        private async Task ChargerClients()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Customers");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _tousLesClients = JsonConvert.DeserializeObject<List<Customer>>(result) ?? new();

                    var tasks = _tousLesClients.Select(c => FetchAndSetAddressForCustomer(c)).ToList();
                    await Task.WhenAll(tasks);

                    dgCustomers.ItemsSource = null;
                    dgCustomers.ItemsSource = _tousLesClients.ToList();
                    SetStatut($"{_tousLesClients.Count} clients chargés", "Green");
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

        // ── Charger l'adresse complète pour affichage dans le DataGrid ──
        private async Task FetchAndSetAddressForCustomer(Customer customer)
        {
            try
            {
                if (customer.IdAdresse <= 0)
                {
                    customer.FullAddress = "Adresse non définie";
                    return;
                }

                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Address/{customer.IdAdresse}");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var address = JsonConvert.DeserializeObject<Address>(result);
                    customer.FullAddress = address?.FullAddress ?? "Adresse non définie";
                }
                else
                {
                    customer.FullAddress = "Adresse non définie";
                }
            }
            catch
            {
                customer.FullAddress = "Adresse non définie";
            }
        }

        // ── Charger l'adresse dans le formulaire à la sélection ──
        private async Task FetchAndSetAddressForm(Customer customer)
        {
            try
            {
                if (customer.IdAdresse <= 0) return;

                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Address/{customer.IdAdresse}");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var address = JsonConvert.DeserializeObject<Address>(result);
                    if (address != null)
                    {
                        txtNumRue.Text = address.StreetNumber;
                        txtNomRue.Text = address.StreetName;
                        txtCodePostal.Text = address.PostalCode;
                        txtVille.Text = address.City;
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatut($"Erreur adresse : {ex.Message}", "Red");
            }
        }

        // ── Recherche locale en temps réel ──
        private void txtSearchCustomer_TextChanged(object sender, TextChangedEventArgs e)
        {
            var terme = txtSearchCustomer.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(terme))
            {
                dgCustomers.ItemsSource = _tousLesClients;
                SetStatut($"{_tousLesClients.Count} clients chargés", "Green");
                return;
            }

            var resultats = _tousLesClients
                .Where(c => c.LastName.ToLower().Contains(terme) || c.FirstName.ToLower().Contains(terme))
                .ToList();

            dgCustomers.ItemsSource = resultats;
            SetStatut(resultats.Count > 0 ? $"{resultats.Count} résultat(s)" : "Aucun résultat",
                      resultats.Count > 0 ? "Green" : "Orange");
        }

        // ── Recherche par nom via l'API ──
        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var nom = txtSearchCustomer.Text.Trim();

            if (string.IsNullOrWhiteSpace(nom))
            {
                await ChargerClients();
                return;
            }

            try
            {
                SetStatut("Recherche en cours...", "Gray");
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/Customers/byname/{Uri.EscapeDataString(nom)}");
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var client = JsonConvert.DeserializeObject<Customer>(result);
                    dgCustomers.ItemsSource = client != null ? new List<Customer> { client } : new List<Customer>();
                    SetStatut(client != null ? "1 résultat" : "Aucun résultat", client != null ? "Green" : "Orange");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    dgCustomers.ItemsSource = null;
                    SetStatut("Aucun client trouvé", "Orange");
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

        // ── Sélection → remplissage formulaire ──
        private async void dgCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer client)
            {
                _clientSelectionne = client;
                _currentAddressId = client.IdAdresse > 0 ? client.IdAdresse : (int?)null;

                txtFirstName.Text = client.FirstName;
                txtLastName.Text = client.LastName;
                txtEmail.Text = client.Email;

                await FetchAndSetAddressForm(client);
            }
        }

        // ── Ajouter un client ──
        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!FormulaireValide()) return;

            try
            {
                // 1. Créer l'adresse
                var newAddress = new
                {
                    StreetNumber = txtNumRue.Text,
                    StreetName = txtNomRue.Text,
                    PostalCode = txtCodePostal.Text,
                    City = txtVille.Text
                };

                var jsonAddress = JsonConvert.SerializeObject(newAddress);
                var contentAddress = new StringContent(jsonAddress, Encoding.UTF8, "application/json");
                var responseAddress = await _httpClient.PostAsync($"{BaseUrl}/api/Address", contentAddress);

                if (!responseAddress.IsSuccessStatusCode)
                {
                    var err = await responseAddress.Content.ReadAsStringAsync();
                    SetStatut($"Erreur création adresse : {err}", "Red");
                    return;
                }

                var addressResult = await responseAddress.Content.ReadAsStringAsync();
                var addressData = JsonConvert.DeserializeObject<Address>(addressResult);

                if (addressData == null || addressData.Id == 0)
                {
                    SetStatut("Erreur : ID adresse non récupéré", "Red");
                    return;
                }

                // 2. Créer le client avec l'IdAdresse
                var body = new
                {
                    Nom = txtLastName.Text,
                    Prenom = txtFirstName.Text,
                    Email = txtEmail.Text,
                    Telephone = "",
                    IdAdresse = addressData.Id
                };

                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/Customers", content);

                if (response.IsSuccessStatusCode)
                {
                    SetStatut("Client ajouté", "Green");
                    BtnClear_Click(sender, e);
                    await ChargerClients();
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    SetStatut($"Erreur ajout : {response.StatusCode} - {err}", "Red");
                }
            }
            catch (Exception ex)
            {
                SetStatut($"Exception : {ex.Message}", "Red");
            }
        }

        // ── Modifier un client ──
        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_clientSelectionne == null)
            {
                SetStatut("Sélectionne un client d'abord", "Orange");
                return;
            }

            if (!FormulaireValide()) return;

            try
            {
                // 1. Mettre à jour l'adresse si elle existe
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
                    var contentAddress = new StringContent(jsonAddress, Encoding.UTF8, "application/json");
                    var responseAddress = await _httpClient.PutAsync($"{BaseUrl}/api/Address/{_currentAddressId}", contentAddress);

                    if (!responseAddress.IsSuccessStatusCode)
                    {
                        SetStatut($"Erreur mise à jour adresse : {responseAddress.StatusCode}", "Red");
                        return;
                    }
                }

                // 2. Mettre à jour le client
                var body = new
                {
                    Nom = txtLastName.Text,
                    Prenom = txtFirstName.Text,
                    Email = txtEmail.Text,
                    Telephone = "",
                    IdAdresse = _currentAddressId ?? 0
                };

                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/api/Customers/{_clientSelectionne.Id}", content);

                if (response.IsSuccessStatusCode)
                {
                    SetStatut("Client modifié", "Green");
                    BtnClear_Click(sender, e);
                    await ChargerClients();
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    SetStatut($"Erreur modification : {response.StatusCode} - {err}", "Red");
                }
            }
            catch (Exception ex)
            {
                SetStatut($"Exception : {ex.Message}", "Red");
            }
        }

        // ── Supprimer un client ──
        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_clientSelectionne == null)
            {
                SetStatut("Sélectionne un client d'abord", "Orange");
                return;
            }

            var confirm = MessageBox.Show(
                $"Supprimer « {txtLastName.Text} {txtFirstName.Text} » ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/api/Customers/{_clientSelectionne.Id}");

                if (response.IsSuccessStatusCode)
                {
                    SetStatut("Client supprimé", "Green");
                    BtnClear_Click(sender, e);
                    await ChargerClients();
                }
                else
                {
                    SetStatut($"Erreur suppression : {response.StatusCode}", "Red");
                }
            }
            catch (Exception ex)
            {
                SetStatut($"Exception : {ex.Message}", "Red");
            }
        }

        // ── Vider le formulaire ──
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            _clientSelectionne = null;
            _currentAddressId = null;
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtNumRue.Text = "";
            txtNomRue.Text = "";
            txtCodePostal.Text = "";
            txtVille.Text = "";
            dgCustomers.SelectedItem = null;
            txtSearchCustomer.Text = "";
            TxtStatut.Text = "";
        }

        // ── Validation ──
        private bool FormulaireValide()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                SetStatut("Remplis au moins le prénom, le nom et l'email", "Orange");
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