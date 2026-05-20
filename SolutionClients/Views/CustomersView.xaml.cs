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
                    dgCustomers.ItemsSource = null;
                    dgCustomers.ItemsSource = _tousLesClients;
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

        // ── Recherche locale en temps réel (TextChanged) ──
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

        // ── Recherche par nom via l'API (bouton Search) ──
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

        // ── Sélection dans la grille → remplissage du formulaire ──
        private void dgCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer client)
            {
                _clientSelectionne = client;
                txtFirstName.Text = client.FirstName;
                txtLastName.Text = client.LastName;
                txtEmail.Text = client.Email;
                txtAdresse.Text = client.Adresse;
            }
        }

        // ── Ajouter un client ──
        private async void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!FormulaireValide()) return;

            try
            {
                var body = new
                {
                    nom = txtLastName.Text,
                    prenom = txtFirstName.Text,
                    email = txtEmail.Text,
                    adresse = txtAdresse.Text
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
                var body = new
                {
                    nom = txtLastName.Text,
                    prenom = txtFirstName.Text,
                    email = txtEmail.Text,
                    adresse = txtAdresse.Text
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
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

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
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtAdresse.Text = "";
            dgCustomers.SelectedItem = null;
            txtSearchCustomer.Text = "";
            TxtStatut.Text = "";
        }

        // ── Validation du formulaire ──
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

        // ── Affichage du statut coloré ──
        private void SetStatut(string message, string couleur)
        {
            TxtStatut.Text = message;
            TxtStatut.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(couleur));
        }
    }
}