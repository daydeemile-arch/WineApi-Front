namespace SolutionClients.Models
{
    internal class Wine
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string Type { get; set; } = "";  // ✅ string et non decimal
        public decimal QuantityStock { get; set; }
        public decimal Threshold { get; set; }
        public int ProviderId { get; set; }
    }
}