namespace SolutionClients.Models
{
    internal class Address
    {
        public int Id { get; set; }
        public string StreetNumber { get; set; } = "";
        public string StreetName { get; set; } = "";
        public string PostalCode { get; set; } = "";
        public string City { get; set; } = "";

        public string FullAddress => $"{StreetNumber} {StreetName}, {PostalCode} {City}";
    }
}