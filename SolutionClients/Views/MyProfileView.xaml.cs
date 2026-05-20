using SolutionClients.Models;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Controls;

namespace SolutionClients.Views
{
    public partial class MyProfileView : UserControl
    {
        private readonly HttpClient _httpClient;

        public MyProfileView(HttpClient httpClient)
        {
            InitializeComponent();

            _httpClient = httpClient;

            LoadProfile();
        }

        private async void LoadProfile()
        {
            try
            {
                int customerId =
     Session.CustomerId;

                var response =
                    await _httpClient.GetAsync(
                        $"api/customers/{customerId}");

                if (response.IsSuccessStatusCode)
                {
                    var json =
                        await response.Content.ReadAsStringAsync();

                    var profile =
                        JsonSerializer.Deserialize<CustomerProfile>(
                            json,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                    txtFirstName.Text = profile.FirstName;
                    txtLastName.Text = profile.LastName;
                    txtEmail.Text = profile.Email;
                    txtStreet.Text = profile.StreetName;
                    txtPostalCode.Text = profile.PostalCode;
                    txtCity.Text = profile.City;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }

    public class CustomerProfile
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string StreetName { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
    }
}