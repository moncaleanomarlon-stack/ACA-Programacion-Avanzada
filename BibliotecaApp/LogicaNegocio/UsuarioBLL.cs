using System.Data;
using BibliotecaApp.AccesoDatos;
using BibliotecaApp.Entidades;

namespace BibliotecaApp.LogicaNegocio
{
    public class UsuarioBLL
    {
        private readonly UsuarioDAL dal = new UsuarioDAL();

        public DataTable Listar(string filtro) { return dal.Listar(filtro); }
        public DataTable ListarParaCombo() { return dal.ListarParaCombo(); }

        private static void Validar(Usuario u)
        {
            u.Documento = Validador.Documento(u.Documento, "Documento");
            u.Nombre = Validador.NombrePersona(u.Nombre, "Nombre", 50);
            u.Apellido = Validador.NombrePersona(u.Apellido, "Apellido", 50);
            u.Telefono = Validador.Telefono(u.Telefono, "Teléfono");
            u.Email = Validador.Correo(u.Email, "Correo electrónico", 100);
        }

        public int Registrar(Usuario usuario)
        {
            Validar(usuario);
            return dal.Insertar(usuario);
        }

        public void Actualizar(Usuario usuario)
        {
            Validar(usuario);
            if (!dal.Actualizar(usuario))
                throw new ReglaNegocioException("El usuario ya no existe; actualice la lista.");
        }

        public void Eliminar(int usuarioId)
        {
            if (!dal.Eliminar(usuarioId))
                throw new ReglaNegocioException("El usuario ya no existe; actualice la lista.");
        }
    }
}
