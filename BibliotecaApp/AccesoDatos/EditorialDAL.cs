using System;
using System.Data;
using BibliotecaApp.Entidades;

namespace BibliotecaApp.AccesoDatos
{
    public class EditorialDAL
    {
        private readonly ConexionBD conexion = new ConexionBD();

        public DataTable Listar(string filtro)
        {
            return conexion.Consultar(
                "SELECT EditorialID, Nombre, Telefono, Email FROM Editoriales " +
                "WHERE Nombre LIKE @f OR Telefono LIKE @f OR Email LIKE @f ORDER BY Nombre",
                ConexionBD.Param("@f", ConexionBD.PatronLike(filtro)));
        }

        public DataTable ListarParaCombo()
        {
            return conexion.Consultar("SELECT EditorialID, Nombre FROM Editoriales ORDER BY Nombre");
        }

        public int Insertar(Editorial editorial)
        {
            object id = conexion.Escalar(
                "INSERT INTO Editoriales (Nombre, Telefono, Email) " +
                "VALUES (@Nombre, @Telefono, @Email); SELECT CAST(SCOPE_IDENTITY() AS INT);",
                ConexionBD.Param("@Nombre", editorial.Nombre),
                ConexionBD.Param("@Telefono", editorial.Telefono),
                ConexionBD.Param("@Email", editorial.Email));
            return Convert.ToInt32(id);
        }

        public bool Actualizar(Editorial editorial)
        {
            return conexion.Ejecutar(
                "UPDATE Editoriales SET Nombre = @Nombre, Telefono = @Telefono, Email = @Email " +
                "WHERE EditorialID = @Id",
                ConexionBD.Param("@Nombre", editorial.Nombre),
                ConexionBD.Param("@Telefono", editorial.Telefono),
                ConexionBD.Param("@Email", editorial.Email),
                ConexionBD.Param("@Id", editorial.EditorialID)) > 0;
        }

        public bool Eliminar(int editorialId)
        {
            return conexion.Ejecutar("DELETE FROM Editoriales WHERE EditorialID = @Id",
                ConexionBD.Param("@Id", editorialId)) > 0;
        }

        public bool Existe(int editorialId)
        {
            return Convert.ToInt32(conexion.Escalar(
                "SELECT COUNT(*) FROM Editoriales WHERE EditorialID = @Id",
                ConexionBD.Param("@Id", editorialId))) > 0;
        }
    }
}
