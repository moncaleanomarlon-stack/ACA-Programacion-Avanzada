using System;
using System.Data;
using BibliotecaApp.AccesoDatos;
using BibliotecaApp.Entidades;

namespace BibliotecaApp.LogicaNegocio
{
    public class LibroBLL
    {
        private readonly LibroDAL dal = new LibroDAL();
        private readonly AutorDAL autorDal = new AutorDAL();
        private readonly EditorialDAL editorialDal = new EditorialDAL();

        public static readonly string[] Criterios = new string[]
        {
            LibroDAL.CriterioTodos, LibroDAL.CriterioIsbn, LibroDAL.CriterioTitulo, LibroDAL.CriterioAutor
        };

        public DataTable Buscar(string criterio, string texto) { return dal.Buscar(criterio, texto); }
        public DataTable ListarParaCombo() { return dal.ListarParaCombo(); }

        private void Validar(Libro l)
        {
            l.ISBN = Validador.Isbn(l.ISBN, "ISBN");
            l.Titulo = Validador.Texto(l.Titulo, "Título", 150);

            if (l.AutorID <= 0) throw new ReglaNegocioException("Debe seleccionar un autor.");
            if (l.EditorialID <= 0) throw new ReglaNegocioException("Debe seleccionar una editorial.");
            if (!autorDal.Existe(l.AutorID))
                throw new ReglaNegocioException("El autor seleccionado no existe.");
            if (!editorialDal.Existe(l.EditorialID))
                throw new ReglaNegocioException("La editorial seleccionada no existe.");

            int anioActual = DateTime.Now.Year;
            if (l.AnioPublicacion <= 1000 || l.AnioPublicacion > anioActual)
                throw new ReglaNegocioException(
                    "El año de publicación debe estar entre 1001 y " + anioActual + ".");
            if (l.Stock < 0 || l.Stock > 100000)
                throw new ReglaNegocioException("Las existencias deben estar entre 0 y 100000.");
        }

        public int Registrar(Libro libro)
        {
            Validar(libro);
            return dal.Insertar(libro);
        }

        public void Actualizar(Libro libro)
        {
            Validar(libro);
            if (!dal.Actualizar(libro))
                throw new ReglaNegocioException("El libro ya no existe; actualice la lista.");
        }

        public void Eliminar(int libroId)
        {
            int activos = dal.ContarPrestamosActivos(libroId);
            if (activos > 0)
                throw new ReglaNegocioException(
                    "No se puede eliminar el libro: tiene " + activos + " préstamo(s) activo(s).");
            // Si tiene historial de préstamos, la FK de la base de datos impide el borrado
            // y ConexionBD traduce el error a un mensaje claro.
            if (!dal.Eliminar(libroId))
                throw new ReglaNegocioException("El libro ya no existe; actualice la lista.");
        }
    }
}
