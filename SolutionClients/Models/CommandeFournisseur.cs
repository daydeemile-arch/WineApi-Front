namespace SolutionClients.Models
{
    public class CommandeFournisseur
    {
        public int IdCommande { get; set; }
        public int IdVin { get; set; }
        public string NomVin { get; set; } = "";
        public int IdFournisseur { get; set; }
        public string NomFournisseur { get; set; } = "";
        public int Quantite { get; set; }
        public string StatutCommande { get; set; } = "";
        public DateTime DateCommande { get; set; }
        public DateTime? DateAcceptation { get; set; }
    }
}