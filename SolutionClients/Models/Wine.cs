using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionClients.Models
{
    internal class Wine
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int QuantityStock { get; set; }
        public int Threshold { get; set; }
        public int ProviderId { get; set; }
    }
}
