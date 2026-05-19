using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using SolutionClients.Views;

namespace SolutionClients.Views
{
    public partial class LoginWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private string? _token;

        private const string BaseUrl = "http://localhost:5120";

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            TxtToken.Text = "Connexion en cours...";
            TxtToken.Foreground = System.Windows.Media.Brushes.Gray;

            try
            {
                var body = new
                {
                    email = TxtEmail.Text,
                    password = TxtPassword.Password
                };

                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/api/auth/login", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic? data = JsonConvert.DeserializeObject(result);
                    _token = data?.token;

                    // Ouvre le Dashboard en lui passant le token et le httpClient
                    var dashboard = new Dashboard(_httpClient, _token!);
                    dashboard.Show();
                    this.Close(); // ferme la fenêtre de login
                }
                else
                {
                    TxtToken.Foreground = System.Windows.Media.Brushes.Red;
                    TxtToken.Text = $"❌ Échec login : {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                TxtToken.Foreground = System.Windows.Media.Brushes.Red;
                TxtToken.Text = $"❌ Erreur : {ex.Message}";
            }
        }
    }
}