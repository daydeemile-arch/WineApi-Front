using Newtonsoft.Json;
using SolutionClients.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Windows;

namespace SolutionClients.Views
{
    public partial class Register : Window
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5120";

        public Register(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
        }
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }   
        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {



            TxtToken.Text = "Création du compte en cours...";
            TxtToken.Foreground = System.Windows.Media.Brushes.Gray;

            // Validation basique côté client
            if (string.IsNullOrWhiteSpace(TxtPrenom.Text) ||
                string.IsNullOrWhiteSpace(TxtNom.Text) ||
                string.IsNullOrWhiteSpace(TxtEmail.Text) ||
                string.IsNullOrWhiteSpace(TxtPassword.Password))
            {
                TxtToken.Text = "❌ Remplis tous les champs obligatoires.";
                TxtToken.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            if (TxtPassword.Password != TxtConfirmPassword.Password)
            {
                TxtToken.Text = "❌ Les mots de passe ne correspondent pas.";
                TxtToken.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            try
            {
                var body = new
                {
                    firstName = TxtPrenom.Text.Trim(),
                    lastName = TxtNom.Text.Trim(),
                    email = TxtEmail.Text.Trim(),
                    password = TxtPassword.Password,
                    confirmPassword = TxtConfirmPassword.Password,
                    telephone = TxtTelephone.Text.Trim(),
                    adresse = TxtAddress.Text.Trim() 
                };

                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/api/auth/register", content);
                var result = await response.Content.ReadAsStringAsync();
                //MessageBox.Show($"Status: {response.StatusCode}\nRéponse: {result}");

                if (response.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<LoginResponse>(result);

                    if (data?.Success == true)
                    {
                        // Injecter le token dans le HttpClient partagé
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", data.Token);

                        TxtToken.Text = "✅ Compte créé avec succès !";
                        TxtToken.Foreground = System.Windows.Media.Brushes.Green;

                        // Ouvrir le Dashboard et fermer le Register
                        var dashboard = new Dashboard(_httpClient, data.Token);
                        dashboard.Show();
                        this.Close();
                    }
                    else
                    {
                        TxtToken.Text = $"❌ {data?.Message}";
                        TxtToken.Foreground = System.Windows.Media.Brushes.Red;
                    }
                }
                else
                {
                    var data = JsonConvert.DeserializeObject<LoginResponse>(result);
                    TxtToken.Text = $"❌ {data?.Message ?? response.StatusCode.ToString()}";
                    TxtToken.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                TxtToken.Text = $"❌ Erreur : {ex.Message}";
                TxtToken.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
    }
}