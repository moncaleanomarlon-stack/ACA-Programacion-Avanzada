namespace BibliotecaApp.Entidades
{
    /// <summary>
    /// Clase base abstracta para las entidades que representan personas
    /// (Autor y Usuario). Justificación de la herencia: ambas comparten
    /// Nombre y Apellido y el mismo criterio para mostrar el nombre completo.
    /// </summary>
    public abstract class Persona
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }

        protected Persona() { }

        protected Persona(string nombre, string apellido)
        {
            Nombre = nombre;
            Apellido = apellido;
        }

        public string NombreCompleto
        {
            get { return (Nombre + " " + Apellido).Trim(); }
        }

        /// <summary>Descripción corta; cada subclase la especializa (polimorfismo).</summary>
        public abstract string Describir();

        public override string ToString()
        {
            return Describir();
        }
    }
}
