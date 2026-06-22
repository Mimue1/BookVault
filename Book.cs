namespace BookVault
{
    public class Book
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int? Year { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}
