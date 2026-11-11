using System;
using System.Data;
using BibliotecaApp.Entidades;

namespace BibliotecaApp.AccesoDatos
{
    public class UsuarioDAL
    {
        private readonly ConexionBD conexion = new ConexionBD();

        public DataTable Listar(string filtro)
        {
            return conexion.Consultar(
                "SELECT UsuarioID, Documento, Nombre, Apellido, Telefono, Email FROM Usuarios " +
                "WHERE Documento LIKE @f OR Nombre LIKE @f OR Apellido LIKE @f OR Email LIKE @f " +
                "ORDER BY Apellido, Nombre",
                ConexionBD.Param("@f", ConexionBD.PatronLike(filtro)));
        }

        public DataTable ListarParaCombo()
        {
            return conexion.Consultar(
                "SELECT UsuarioID, CONCAT(Documento, ' - ', Nombre, ' ', Apellido) AS Descripcion " +
                "FROM Usuarios ORDER BY Apellido, Nombre");
        }

        public int Insertar(Usuario u)
        {
            object id = conexion.Escalar(
                "INSERT INTO Usuarios (Documento, Nombre, Apellido, Telefono, Email) " +
                "VALUES (@Documento, @Nombre, @Apellido, @Telefono, @Email); " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT);",
                ConexionBD.Param("@Documento", u.Documento),
                ConexionBD.Param("@Nombre", u.Nombre),
                ConexionBD.Param("@Apellido", u.Apellido),
                ConexionBD.Param("@Telefono", u.Telefono),
                ConexionBD.Param("@Email", u.Email));
            return Convert.ToInt32(id);
        }

        public bool Actualizar(Usuario u)
        {
            return conexion.Ejecutar(
                "UPDATE Usuarios SET Documento = @Documento, Nombre = @Nombre, Apellido = @Apellido, " +
                "Telefono = @Telefono, Email = @Email WHERE UsuarioID = @Id",
                ConexionBD.Param("@Documento", u.Documento),
                ConexionBD.Param("@Nombre", u.Nombre),
                ConexionBD.Param("@Apellido", u.Apellido),
                ConexionBD.Param("@Telefono", u.Telefono),
                ConexionBD.Param("@Email", u.Email),
                ConexionBD.Param("@Id", u.UsuarioID)) > 0;
        }

        public bool Eliminar(int usuarioId)
        {
            return conexion.Ejecutar("DELETE FROM Usuarios WHERE UsuarioID = @Id",
                ConexionBD.Param("@Id", usuarioId)) > 0;
        }

        public bool Existe(int usuarioId)
        {
            return Convert.ToInt32(conexion.Escalar(
                "SELECT COUNT(*) FROM Usuarios WHERE UsuarioID = @Id",
                ConexionBD.Param("@Id", usuarioId))) > 0;
        }
    }
}
