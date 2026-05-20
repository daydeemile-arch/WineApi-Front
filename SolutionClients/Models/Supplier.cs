namespace SolutionClients.Models
{
    internal class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public int IdAdress { get; set; }
        public string FullAddress { get; set; } = "";
    }
}
