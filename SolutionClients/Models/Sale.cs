using System;
using System.Collections.Generic;

namespace SolutionClients.Models
{
    public class SaleLine
    {
        public int Id { get; set; }
        public int WineId { get; set; }
        public string WineName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal => UnitPrice * Quantity;
    }

    public class Sale
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string CustomerName { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public DateTime DateSale { get; set; }
        public List<SaleLine> Lines { get; set; } = new();
    }
}