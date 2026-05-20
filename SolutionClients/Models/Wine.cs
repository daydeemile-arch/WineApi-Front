namespace SolutionClients.Models
{
    public class Wine
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string Type { get; set; } = "";
        public decimal QuantityStock { get; set; }
        public decimal Threshold { get; set; }
        public int ProviderId { get; set; }
        public int QuantiteCommande { get; set; }

        // Propriété calculée pour le style de ligne
        public bool IsBelowThreshold => QuantityStock <= Threshold;
    }
}
