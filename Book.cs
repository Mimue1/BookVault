namespace Bücherverwaltung
{
    public class Book
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public decimal? Preis { get; set; }
        public int? Datum { get; set; }
        public string Kategorie { get; set; } = string.Empty;
    }
}
