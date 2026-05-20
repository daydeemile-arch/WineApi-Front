using System;
using System.Collections.Generic;
using System.Text;

namespace SolutionClients.Models
{
    internal class Customer
    {
        public int Id { get; set; }
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Adresse { get; set; } = "";


    }
}
