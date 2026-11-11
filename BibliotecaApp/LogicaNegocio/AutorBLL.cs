using System.Data;
using BibliotecaApp.AccesoDatos;
using BibliotecaApp.Entidades;

namespace BibliotecaApp.LogicaNegocio
{
    public class AutorBLL
    {
        private readonly AutorDAL dal = new AutorDAL();

        public DataTable Listar(string filtro) { return dal.Listar(filtro); }
        public DataTable ListarParaCombo() { return dal.ListarParaCombo(); }

        private static void Validar(Autor a)
        {
            a.Nombre = Validador.NombrePersona(a.Nombre, "Nombre", 50);
            a.Apellido = Validador.NombrePersona(a.Apellido, "Apellido", 50);
            a.Nacionalidad = Validador.NombrePersona(a.Nacionalidad, "Nacionalidad", 50);
        }

        public int Registrar(Autor autor)
        {
            Validar(autor);
            return dal.Insertar(autor);
        }

        public void Actualizar(Autor autor)
        {
            Validar(autor);
            if (!dal.Actualizar(autor))
                throw new ReglaNegocioException("El autor ya no existe; actualice la lista.");
        }

        public void Eliminar(int autorId)
        {
            if (!dal.Eliminar(autorId))
                throw new ReglaNegocioException("El autor ya no existe; actualice la lista.");
        }
    }
}
