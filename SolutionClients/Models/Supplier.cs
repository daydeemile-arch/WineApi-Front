using System.ComponentModel;

namespace SolutionClients.Models
{
    internal class Supplier : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public int IdAdress { get; set; }

        private string _fullAddress = "";
        public string FullAddress
        {
            get => _fullAddress;
            set
            {
                _fullAddress = value;
                OnPropertyChanged(nameof(FullAddress));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}