namespace BibliotecaApp.Entidades
{
    public class Editorial
    {
        public int EditorialID { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }

        public Editorial() { }

        public Editorial(int id, string nombre, string telefono, string email)
        {
            EditorialID = id;
            Nombre = nombre;
            Telefono = telefono;
            Email = email;
        }

        public override string ToString()
        {
            return Nombre;
        }
    }
}
