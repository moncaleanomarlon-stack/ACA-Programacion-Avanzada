using System;
using System.Data;
using BibliotecaApp.Entidades;

namespace BibliotecaApp.AccesoDatos
{
    public class LibroDAL
    {
        public const string CriterioTodos = "Todos";
        public const string CriterioIsbn = "Código (ISBN)";
        public const string CriterioTitulo = "Título";
        public const string CriterioAutor = "Autor";

        private readonly ConexionBD conexion = new ConexionBD();

        private const string SelectBase =
            "SELECT l.LibroID, l.ISBN, l.Titulo, l.AutorID, " +
            "CONCAT(a.Nombre, ' ', a.Apellido) AS Autor, l.EditorialID, e.Nombre AS Editorial, " +
            "l.AnioPublicacion, l.Stock AS Disponibles " +
            "FROM Libros l " +
            "INNER JOIN Autores a ON l.AutorID = a.AutorID " +
            "INNER JOIN Editoriales e ON l.EditorialID = e.EditorialID ";

        /// <summary>Busca libros por criterio (código, título, autor o todos).</summary>
        public DataTable Buscar(string criterio, string texto)
        {
            string where;
            switch (criterio)
            {
                case CriterioIsbn:   where = "WHERE l.ISBN LIKE @f "; break;
                case CriterioTitulo: where = "WHERE l.Titulo LIKE @f "; break;
                case CriterioAutor:  where = "WHERE CONCAT(a.Nombre, ' ', a.Apellido) LIKE @f "; break;
                default:
                    where = "WHERE l.ISBN LIKE @f OR l.Titulo LIKE @f " +
                            "OR CONCAT(a.Nombre, ' ', a.Apellido) LIKE @f OR e.Nombre LIKE @f ";
                    break;
            }
            return conexion.Consultar(SelectBase + where + "ORDER BY l.Titulo",
                ConexionBD.Param("@f", ConexionBD.PatronLike(texto)));
        }

        public DataTable ListarParaCombo()
        {
            return conexion.Consultar(
                "SELECT LibroID, CONCAT(Titulo, ' (', ISBN, ') - Disp: ', Stock) AS Descripcion " +
                "FROM Libros ORDER BY Titulo");
        }

        public int Insertar(Libro l)
        {
            object id = conexion.Escalar(
                "INSERT INTO Libros (ISBN, Titulo, AutorID, EditorialID, AnioPublicacion, Stock) " +
                "VALUES (@ISBN, @Titulo, @AutorID, @EditorialID, @Anio, @Stock); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                ConexionBD.Param("@ISBN", l.ISBN),
                ConexionBD.Param("@Titulo", l.Titulo),
                ConexionBD.Param("@AutorID", l.AutorID),
                ConexionBD.Param("@EditorialID", l.EditorialID),
                ConexionBD.Param("@Anio", l.AnioPublicacion),
                ConexionBD.Param("@Stock", l.Stock));
            return Convert.ToInt32(id);
        }

        public bool Actualizar(Libro l)
        {
            return conexion.Ejecutar(
                "UPDATE Libros SET ISBN = @ISBN, Titulo = @Titulo, AutorID = @AutorID, " +
                "EditorialID = @EditorialID, AnioPublicacion = @Anio, Stock = @Stock " +
                "WHERE LibroID = @Id",
                ConexionBD.Param("@ISBN", l.ISBN),
                ConexionBD.Param("@Titulo", l.Titulo),
                ConexionBD.Param("@AutorID", l.AutorID),
                ConexionBD.Param("@EditorialID", l.EditorialID),
                ConexionBD.Param("@Anio", l.AnioPublicacion),
                ConexionBD.Param("@Stock", l.Stock),
                ConexionBD.Param("@Id", l.LibroID)) > 0;
        }

        public bool Eliminar(int libroId)
        {
            return conexion.Ejecutar("DELETE FROM Libros WHERE LibroID = @Id",
                ConexionBD.Param("@Id", libroId)) > 0;
        }

        /// <summary>Cantidad de préstamos ACTIVOS del libro (ejemplares fuera de la biblioteca).</summary>
        public int ContarPrestamosActivos(int libroId)
        {
            return Convert.ToInt32(conexion.Escalar(
                "SELECT COUNT(*) FROM Prestamos WHERE LibroID = @Id AND Estado = 'Prestado'",
                ConexionBD.Param("@Id", libroId)));
        }
    }
}
