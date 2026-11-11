namespace BibliotecaApp.Entidades
{
    public class Libro
    {
        public int LibroID { get; set; }
        public string ISBN { get; set; }
        public string Titulo { get; set; }
        public int AutorID { get; set; }
        public int EditorialID { get; set; }
        public int AnioPublicacion { get; set; }

        /// <summary>Ejemplares disponibles actualmente para préstamo.</summary>
        public int Stock { get; set; }

        public Libro() { }

        public Libro(int id, string isbn, string titulo, int autorId, int editorialId, int anio, int stock)
        {
            LibroID = id;
            ISBN = isbn;
            Titulo = titulo;
            AutorID = autorId;
            EditorialID = editorialId;
            AnioPublicacion = anio;
            Stock = stock;
        }

        public bool TieneExistencias
        {
            get { return Stock > 0; }
        }

        public override string ToString()
        {
            return Titulo + " (" + ISBN + ")";
        }
    }
}
