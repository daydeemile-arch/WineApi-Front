using System.Windows;

namespace SolutionClients.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer le nom d'utilisateur depuis le champ TextBox
            string username = txtUsername.Text;

            // Récupérer le mot de passe depuis le champ PasswordBox
            string password = txtPassword.Password;

            //Vérifier les informations d'identification
            //à finir !! if (username == "admin" && password == "password") // Exemple de validation simple

            Dashboard dashboard = new Dashboard();
            dashboard.Show();

            this.Close();
        }
    }
}