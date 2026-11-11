using System;
using System.Data;
using Microsoft.Data.SqlClient;
using BibliotecaApp.Entidades;

namespace BibliotecaApp.AccesoDatos
{
    /// <summary>
    /// Acceso a datos de préstamos. Todas las operaciones que modifican el
    /// préstamo Y el stock del libro se ejecutan dentro de una transacción
    /// para garantizar la consistencia (o se aplican ambas, o ninguna).
    /// </summary>
    public class PrestamoDAL
    {
        public const string FiltroTodos = "Todos";

        private readonly ConexionBD conexion = new ConexionBD();

        public DataTable Listar(string estado, string texto)
        {
            return conexion.Consultar(
                "SELECT p.PrestamoID, p.UsuarioID, u.Documento, " +
                "CONCAT(u.Nombre, ' ', u.Apellido) AS Usuario, " +
                "p.LibroID, l.ISBN, l.Titulo AS Libro, " +
                "p.FechaPrestamo, p.FechaDevolucion, p.Estado " +
                "FROM Prestamos p " +
                "INNER JOIN Usuarios u ON p.UsuarioID = u.UsuarioID " +
                "INNER JOIN Libros l ON p.LibroID = l.LibroID " +
                "WHERE (@Estado = 'Todos' OR p.Estado = @Estado) " +
                "AND (u.Documento LIKE @f OR CONCAT(u.Nombre, ' ', u.Apellido) LIKE @f " +
                "     OR l.Titulo LIKE @f OR l.ISBN LIKE @f) " +
                "ORDER BY p.FechaPrestamo DESC, p.PrestamoID DESC",
                ConexionBD.Param("@Estado", string.IsNullOrEmpty(estado) ? FiltroTodos : estado),
                ConexionBD.Param("@f", ConexionBD.PatronLike(texto)));
        }

        public int ContarActivosDeUsuario(int usuarioId)
        {
            return Convert.ToInt32(conexion.Escalar(
                "SELECT COUNT(*) FROM Prestamos WHERE UsuarioID = @U AND Estado = 'Prestado'",
                ConexionBD.Param("@U", usuarioId)));
        }

        public bool ExistePrestamoActivo(int usuarioId, int libroId)
        {
            return Convert.ToInt32(conexion.Escalar(
                "SELECT COUNT(*) FROM Prestamos WHERE UsuarioID = @U AND LibroID = @L AND Estado = 'Prestado'",
                ConexionBD.Param("@U", usuarioId),
                ConexionBD.Param("@L", libroId))) > 0;
        }

        // ------------------------------------------------------------------
        // Auxiliares para trabajar dentro de una transacción
        // ------------------------------------------------------------------
        private static SqlCommand Comando(string sql, SqlConnection con, SqlTransaction tx,
                                          params SqlParameter[] parametros)
        {
            SqlCommand cmd = new SqlCommand(sql, con, tx);
            if (parametros != null) cmd.Parameters.AddRange(parametros);
            return cmd;
        }

        /// <summary>
        /// Abre la conexión y la transacción, ejecuta la operación y confirma.
        /// Ante cualquier error deshace los cambios. Si el Rollback también falla
        /// (p. ej. se perdió la conexión) se conserva el error original, que es el
        /// que explica qué pasó. Los errores de SQL Server se traducen a mensajes claros.
        /// </summary>
        private T EnTransaccion<T>(Func<SqlConnection, SqlTransaction, T> operacion)
        {
            try
            {
                using (SqlConnection con = conexion.ObtenerConexion())
                {
                    con.Open();
                    using (SqlTransaction tx = con.BeginTransaction())
                    {
                        try
                        {
                            T resultado = operacion(con, tx);
                            tx.Commit();
                            return resultado;
                        }
                        catch
                        {
                            try { tx.Rollback(); }
                            catch (Exception) { /* se relanza el error original */ }
                            throw;
                        }
                    }
                }
            }
            catch (SqlException ex) { throw ConexionBD.Traducir(ex); }
        }

        private void EnTransaccion(Action<SqlConnection, SqlTransaction> operacion)
        {
            EnTransaccion<bool>((con, tx) => { operacion(con, tx); return true; });
        }

        // ------------------------------------------------------------------
        // Operaciones
        // ------------------------------------------------------------------

        /// <summary>
        /// Registra el préstamo y descuenta una unidad del stock (transacción).
        /// Las reglas "mismo libro activo" y "máximo de préstamos activos" se
        /// verifican aquí, bloqueando los préstamos del usuario, para que dos
        /// registros simultáneos no puedan saltárselas.
        /// </summary>
        public int RegistrarPrestamo(int usuarioId, int libroId, DateTime fechaPrestamo, int maxActivosPorUsuario)
        {
            return EnTransaccion((con, tx) =>
            {
                // 1. El usuario debe existir
                int usuarios = Convert.ToInt32(Comando(
                    "SELECT COUNT(*) FROM Usuarios WHERE UsuarioID = @U", con, tx,
                    ConexionBD.Param("@U", usuarioId)).ExecuteScalar());
                if (usuarios == 0)
                    throw new ReglaNegocioException("El usuario seleccionado no existe.");

                // 2. Préstamos activos del usuario. UPDLOCK + HOLDLOCK bloquea ese rango
                //    hasta el Commit: otro préstamo del mismo usuario espera su turno.
                DataTable activos = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(Comando(
                    "SELECT LibroID FROM Prestamos WITH (UPDLOCK, HOLDLOCK) " +
                    "WHERE UsuarioID = @U AND Estado = 'Prestado'", con, tx,
                    ConexionBD.Param("@U", usuarioId))))
                {
                    da.Fill(activos);
                }
                foreach (DataRow fila in activos.Rows)
                {
                    if ((int)fila["LibroID"] == libroId)
                        throw new ReglaNegocioException(
                            "El usuario ya tiene prestado este libro y aún no lo ha devuelto.");
                }
                if (activos.Rows.Count >= maxActivosPorUsuario)
                    throw new ReglaNegocioException(
                        "El usuario alcanzó el máximo de " + maxActivosPorUsuario +
                        " préstamos activos. Debe devolver un libro antes de solicitar otro.");

                // 3. El libro debe existir; se bloquea la fila para evitar que dos
                //    préstamos simultáneos consuman la misma última existencia.
                object stockObj = Comando(
                    "SELECT Stock FROM Libros WITH (UPDLOCK, ROWLOCK) WHERE LibroID = @L", con, tx,
                    ConexionBD.Param("@L", libroId)).ExecuteScalar();
                if (stockObj == null)
                    throw new ReglaNegocioException("El libro seleccionado no existe.");
                if (Convert.ToInt32(stockObj) <= 0)
                    throw new ReglaNegocioException(
                        "El libro seleccionado no cuenta con existencias disponibles.");

                // 4. Insertar préstamo
                int id = Convert.ToInt32(Comando(
                    "INSERT INTO Prestamos (UsuarioID, LibroID, FechaPrestamo, FechaDevolucion, Estado) " +
                    "VALUES (@U, @L, @F, NULL, 'Prestado'); SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    con, tx,
                    ConexionBD.Param("@U", usuarioId),
                    ConexionBD.Param("@L", libroId),
                    ConexionBD.Param("@F", fechaPrestamo)).ExecuteScalar());

                // 5. Descontar stock
                Comando("UPDATE Libros SET Stock = Stock - 1 WHERE LibroID = @L", con, tx,
                    ConexionBD.Param("@L", libroId)).ExecuteNonQuery();

                return id;
            });
        }

        /// <summary>Registra la devolución: estado 'Devuelto', fecha y +1 al stock (transacción).</summary>
        public void RegistrarDevolucion(int prestamoId, DateTime fechaDevolucion)
        {
            EnTransaccion((con, tx) =>
            {
                // El libro y el estado se leen del propio préstamo (no del formulario)
                DataTable datos = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(Comando(
                    "SELECT LibroID, Estado, FechaPrestamo FROM Prestamos WITH (UPDLOCK, ROWLOCK) " +
                    "WHERE PrestamoID = @P", con, tx, ConexionBD.Param("@P", prestamoId))))
                {
                    da.Fill(datos);
                }
                if (datos.Rows.Count == 0)
                    throw new ReglaNegocioException("El préstamo seleccionado no existe.");

                DataRow fila = datos.Rows[0];
                if ((string)fila["Estado"] == Prestamo.EstadoDevuelto)
                    throw new ReglaNegocioException("Este préstamo ya fue devuelto anteriormente.");
                if (fechaDevolucion < (DateTime)fila["FechaPrestamo"])
                    throw new ReglaNegocioException(
                        "La fecha de devolución no puede ser anterior a la fecha del préstamo.");

                int libroId = (int)fila["LibroID"];

                Comando("UPDATE Prestamos SET FechaDevolucion = @F, Estado = 'Devuelto' WHERE PrestamoID = @P",
                    con, tx,
                    ConexionBD.Param("@F", fechaDevolucion),
                    ConexionBD.Param("@P", prestamoId)).ExecuteNonQuery();

                Comando("UPDATE Libros SET Stock = Stock + 1 WHERE LibroID = @L", con, tx,
                    ConexionBD.Param("@L", libroId)).ExecuteNonQuery();
            });
        }

        /// <summary>
        /// Edita las fechas de un préstamo manteniendo la coherencia con su estado:
        /// 'Prestado' no admite fecha de devolución; 'Devuelto' la exige.
        /// </summary>
        public void ActualizarFechas(int prestamoId, DateTime fechaPrestamo, DateTime? fechaDevolucion)
        {
            EnTransaccion((con, tx) =>
            {
                object estadoObj = Comando(
                    "SELECT Estado FROM Prestamos WITH (UPDLOCK) WHERE PrestamoID = @P", con, tx,
                    ConexionBD.Param("@P", prestamoId)).ExecuteScalar();
                if (estadoObj == null)
                    throw new ReglaNegocioException("El préstamo seleccionado no existe.");

                bool devuelto = (string)estadoObj == Prestamo.EstadoDevuelto;
                if (!devuelto && fechaDevolucion.HasValue)
                    throw new ReglaNegocioException(
                        "Un préstamo en estado 'Prestado' no puede tener fecha de devolución. " +
                        "Use la opción 'Registrar devolución'.");
                if (devuelto && !fechaDevolucion.HasValue)
                    throw new ReglaNegocioException(
                        "Un préstamo 'Devuelto' debe conservar su fecha de devolución.");

                Comando("UPDATE Prestamos SET FechaPrestamo = @FP, FechaDevolucion = @FD WHERE PrestamoID = @P",
                    con, tx,
                    ConexionBD.Param("@FP", fechaPrestamo),
                    ConexionBD.Param("@FD", fechaDevolucion.HasValue ? (object)fechaDevolucion.Value : null),
                    ConexionBD.Param("@P", prestamoId)).ExecuteNonQuery();
            });
        }

        /// <summary>
        /// Elimina el préstamo. Si estaba activo ('Prestado') devuelve la unidad al
        /// stock para no dejar el inventario inconsistente (transacción).
        /// </summary>
        public void Eliminar(int prestamoId)
        {
            EnTransaccion((con, tx) =>
            {
                DataTable datos = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(Comando(
                    "SELECT LibroID, Estado FROM Prestamos WITH (UPDLOCK, ROWLOCK) WHERE PrestamoID = @P",
                    con, tx, ConexionBD.Param("@P", prestamoId))))
                {
                    da.Fill(datos);
                }
                if (datos.Rows.Count == 0)
                    throw new ReglaNegocioException("El préstamo seleccionado no existe.");

                if ((string)datos.Rows[0]["Estado"] == Prestamo.EstadoPrestado)
                {
                    Comando("UPDATE Libros SET Stock = Stock + 1 WHERE LibroID = @L", con, tx,
                        ConexionBD.Param("@L", (int)datos.Rows[0]["LibroID"])).ExecuteNonQuery();
                }

                Comando("DELETE FROM Prestamos WHERE PrestamoID = @P", con, tx,
                    ConexionBD.Param("@P", prestamoId)).ExecuteNonQuery();
            });
        }
    }
}
