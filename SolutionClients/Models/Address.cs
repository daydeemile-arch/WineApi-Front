namespace SolutionClients.Models
{
    internal class Address
    {
        public int Id { get; set; }
        public string NumRue { get; set; } = "";
        public string NomRue { get; set; } = "";
        public string CodePostal { get; set; } = "";
        public string Ville { get; set; } = "";

        public string FullAddress => $"{NumRue} {NomRue}, {CodePostal} {Ville}";
    }
}
