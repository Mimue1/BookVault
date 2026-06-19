namespace Bücherverwaltung
{
    public class Buch
    {
        public string Name { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public double? Preis { get; set; }
        public int? Datum { get; set; }
        public string Kategorie { get; set; } = string.Empty;
    }
}
