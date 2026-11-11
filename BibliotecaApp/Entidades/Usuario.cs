namespace BibliotecaApp.Entidades
{
    public class Usuario : Persona
    {
        public int UsuarioID { get; set; }
        public string Documento { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }

        public Usuario() { }

        public Usuario(int id, string documento, string nombre, string apellido,
                       string telefono, string email)
            : base(nombre, apellido)
        {
            UsuarioID = id;
            Documento = documento;
            Telefono = telefono;
            Email = email;
        }

        public override string Describir()
        {
            return NombreCompleto + " - Doc. " + Documento;
        }
    }
}
