using System;
using System.Data;

namespace BibliotecaApp.AccesoDatos
{
    /// <summary>Consultas de solo lectura sobre las vistas de reportes del script SQL.</summary>
    public class ReporteDAL
    {
        public static readonly string[] Nombres = new string[]
        {
            "Resumen consolidado de la biblioteca",
            "Listado de libros",
            "Listado de autores",
            "Listado de editoriales",
            "Listado de usuarios",
            "Préstamos activos",
            "Préstamos devueltos",
            "Historial de préstamos",
            "Libros más prestados"
        };

        private readonly ConexionBD conexion = new ConexionBD();

        public DataTable Obtener(string nombreReporte)
        {
            string sql;
            switch (nombreReporte)
            {
                case "Resumen consolidado de la biblioteca":
                    sql = "SELECT Concepto, Total FROM VISTA_ResumenGeneral"; break;
                case "Listado de libros":
                    sql = "SELECT * FROM VISTA_InventarioLibros ORDER BY Titulo"; break;
                case "Listado de autores":
                    sql = "SELECT * FROM VISTA_ResumenAutores ORDER BY Apellido, Nombre"; break;
                case "Listado de editoriales":
                    sql = "SELECT * FROM VISTA_ResumenEditoriales ORDER BY Nombre"; break;
                case "Listado de usuarios":
                    sql = "SELECT UsuarioID, Documento, Nombre, Apellido, Telefono, Email " +
                          "FROM Usuarios ORDER BY Apellido, Nombre"; break;
                case "Préstamos activos":
                    sql = "SELECT * FROM VISTA_PrestamosActivos ORDER BY FechaPrestamo"; break;
                case "Préstamos devueltos":
                    sql = "SELECT * FROM VISTA_HistorialPrestamos WHERE Estado = 'Devuelto' " +
                          "ORDER BY FechaDevolucion DESC"; break;
                case "Historial de préstamos":
                    sql = "SELECT * FROM VISTA_HistorialPrestamos ORDER BY FechaPrestamo DESC"; break;
                case "Libros más prestados":
                    sql = "SELECT * FROM VISTA_LibrosMasPrestados ORDER BY TotalPrestamos DESC, Titulo"; break;
                default:
                    throw new ArgumentException("Reporte no reconocido: " + nombreReporte);
            }
            return conexion.Consultar(sql);
        }
    }
}
