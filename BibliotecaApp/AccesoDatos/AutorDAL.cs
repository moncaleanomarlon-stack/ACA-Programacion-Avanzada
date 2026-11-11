using System;
using System.Data;
using BibliotecaApp.Entidades;

namespace BibliotecaApp.AccesoDatos
{
    public class AutorDAL
    {
        private readonly ConexionBD conexion = new ConexionBD();

        public DataTable Listar(string filtro)
        {
            return conexion.Consultar(
                "SELECT AutorID, Nombre, Apellido, Nacionalidad FROM Autores " +
                "WHERE Nombre LIKE @f OR Apellido LIKE @f OR Nacionalidad LIKE @f " +
                "ORDER BY Apellido, Nombre",
                ConexionBD.Param("@f", ConexionBD.PatronLike(filtro)));
        }

        public DataTable ListarParaCombo()
        {
            return conexion.Consultar(
                "SELECT AutorID, CONCAT(Nombre, ' ', Apellido) AS NombreCompleto " +
                "FROM Autores ORDER BY Apellido, Nombre");
        }

        public int Insertar(Autor autor)
        {
            object id = conexion.Escalar(
                "INSERT INTO Autores (Nombre, Apellido, Nacionalidad) " +
                "VALUES (@Nombre, @Apellido, @Nacionalidad); SELECT CAST(SCOPE_IDENTITY() AS INT);",
                ConexionBD.Param("@Nombre", autor.Nombre),
                ConexionBD.Param("@Apellido", autor.Apellido),
                ConexionBD.Param("@Nacionalidad", autor.Nacionalidad));
            return Convert.ToInt32(id);
        }

        public bool Actualizar(Autor autor)
        {
            return conexion.Ejecutar(
                "UPDATE Autores SET Nombre = @Nombre, Apellido = @Apellido, " +
                "Nacionalidad = @Nacionalidad WHERE AutorID = @Id",
                ConexionBD.Param("@Nombre", autor.Nombre),
                ConexionBD.Param("@Apellido", autor.Apellido),
                ConexionBD.Param("@Nacionalidad", autor.Nacionalidad),
                ConexionBD.Param("@Id", autor.AutorID)) > 0;
        }

        public bool Eliminar(int autorId)
        {
            return conexion.Ejecutar("DELETE FROM Autores WHERE AutorID = @Id",
                ConexionBD.Param("@Id", autorId)) > 0;
        }

        public bool Existe(int autorId)
        {
            return Convert.ToInt32(conexion.Escalar(
                "SELECT COUNT(*) FROM Autores WHERE AutorID = @Id",
                ConexionBD.Param("@Id", autorId))) > 0;
        }
    }
}
