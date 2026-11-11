using System.Data;
using BibliotecaApp.AccesoDatos;
using BibliotecaApp.Entidades;

namespace BibliotecaApp.LogicaNegocio
{
    public class EditorialBLL
    {
        private readonly EditorialDAL dal = new EditorialDAL();

        public DataTable Listar(string filtro) { return dal.Listar(filtro); }
        public DataTable ListarParaCombo() { return dal.ListarParaCombo(); }

        private static void Validar(Editorial e)
        {
            e.Nombre = Validador.Texto(e.Nombre, "Nombre", 100);
            e.Telefono = Validador.Telefono(e.Telefono, "Teléfono");
            e.Email = Validador.Correo(e.Email, "Correo electrónico", 100);
        }

        public int Registrar(Editorial editorial)
        {
            Validar(editorial);
            return dal.Insertar(editorial);
        }

        public void Actualizar(Editorial editorial)
        {
            Validar(editorial);
            if (!dal.Actualizar(editorial))
                throw new ReglaNegocioException("La editorial ya no existe; actualice la lista.");
        }

        public void Eliminar(int editorialId)
        {
            if (!dal.Eliminar(editorialId))
                throw new ReglaNegocioException("La editorial ya no existe; actualice la lista.");
        }
    }
}
