using System;

namespace SolutionClients.Models
{
    internal class Sale
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string CustomerName { get; set; } = "";
        public int WineId { get; set; }
        public string WineName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime SaleDate { get; set; }
    }
}
