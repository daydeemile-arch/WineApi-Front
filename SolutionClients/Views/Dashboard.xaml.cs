using System.Windows;

namespace SolutionClients.Views
{
    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private void BtnWines_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new WinesView();
        }

        private void BtnCustomers_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new CustomersView();
        }

        private void BtnSuppliers_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new SuppliersView();
        }

        private void BtnSales_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new SalesView();
        }

        private void BtnStock_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new StockView();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();

            this.Close();
        }
    }
}