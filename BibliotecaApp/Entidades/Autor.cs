namespace BibliotecaApp.Entidades
{
    public class Autor : Persona
    {
        public int AutorID { get; set; }
        public string Nacionalidad { get; set; }

        public Autor() { }

        public Autor(int id, string nombre, string apellido, string nacionalidad)
            : base(nombre, apellido)
        {
            AutorID = id;
            Nacionalidad = nacionalidad;
        }

        public override string Describir()
        {
            return NombreCompleto + " (" + Nacionalidad + ")";
        }
    }
}
